﻿namespace Nhs.Appointments.Core.UnitTests.AvailabilityCalculations;

public class MultiServiceTests : AvailabilityCalculationsBase
{
    [Fact]
    public async Task MultipleServicesByCreatedDate()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Blue", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Green", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Green", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("5", "Blue", avStatus: "Orphaned", creationOrder: 7),
            TestBooking("6", "Green", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("7", "Blue", avStatus: "Orphaned", creationOrder: 5)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 2),
            TestSession("09:00", "10:00", ["Green"], capacity: 1),
            TestSession("09:00", "10:00", ["Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        // Bookings 1, 2, 3, 6 and 7 should be supported
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "6", "7");

        // Bookings 4 and 5 should not be, because they were created after 6 and 7
        resultingAvailabilityState.Recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        resultingAvailabilityState.Recalculations.Should().NotContain(r => r.Booking.Reference == "5");
    }
    
    /// <summary>
    /// Check that if there is only one candidate slot, it finds it early
    /// </summary>
    [Fact]
    public async Task BestFitModel_PerformanceCheck_1()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue", "Orange", "Black", "Grey"], capacity: 1),
            TestSession("09:00", "10:00", ["Pink"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));
        
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1");
    }

    [Fact]
    public async Task TheBestFitProblem()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Orange"], capacity: 1),
            TestSession("09:00", "09:10", ["Orange", "Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3");
    }

    [Fact]
    public async Task TheBestFitProblem_2()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "C", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "B", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "B", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "A", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "A", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "D", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "A", avStatus: "Orphaned", creationOrder: 7),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["A", "B", "C"], capacity: 1),
            TestSession("09:00", "09:10", ["A", "B", "D"], capacity: 2),
            TestSession("09:00", "09:10", ["A", "C", "D"], capacity: 2),
            TestSession("09:00", "09:10", ["B", "C", "D"], capacity: 2),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5", "6", "7");
    }

    [Fact]
    [InlineData(false)]
    [InlineData(true, true)]
    public async Task TheBestFitProblem_3()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "B", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "A", avStatus: "Orphaned", creationOrder: 2)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["A", "B"], capacity: 1),
            TestSession("09:00", "09:10", ["B", "C", "D"], capacity: 4)
        };

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2");
    }

    [Fact]
    [InlineData(false)]
    [InlineData(true, true)]
    public async Task TheBestFitProblem_4()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "B", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "A", avStatus: "Orphaned", creationOrder: 2)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["A", "B"], capacity: 1),
            TestSession("09:00", "09:10", ["B", "C", "D"], capacity: 4)
        };

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2");
    }

    [Fact]
    [InlineData(false)]
    [InlineData(true, true)]
    public async Task TheBestFitProblem_5()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "B", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "E", avStatus: "Orphaned", creationOrder: 2)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["A", "B", "C", "E"], capacity: 1),
            TestSession("09:00", "09:10", ["A", "D"], capacity: 1),
            TestSession("09:00", "09:10", ["A", "F"], capacity: 1),
            TestSession("09:00", "09:10", ["B", "C"], capacity: 90)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2");
    }

    [Fact]
    [InlineData(false)]
    [InlineData(true, true)]
    public async Task TheBestFitProblem_6()
    {
        //purple blue green
        var bookings = new List<Booking>
        {
            TestBooking("1", "Purple", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Green", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Purple", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Green", avStatus: "Orphaned", creationOrder: 5)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Green"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Purple", "Green"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5");
    }


    /// <summary>
    ///     Prove adding extra orphaned bookings doesn't change the booked status of the first one
    /// </summary>
    [Fact]
    public async Task TheBestFitProblem_7()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Green", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Purple", avStatus: "Orphaned", creationOrder: 3),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Purple", "Orange"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState1 =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        resultingAvailabilityState1.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1");

        resultingAvailabilityState1.Recalculations.Should().NotContain(r => r.Booking.Reference == "2");
        resultingAvailabilityState1.Recalculations.Should().NotContain(r => r.Booking.Reference == "3");

        //now append orange orphaned bookings on, to make orange now the higher opportunity cost service
        bookings.AddRange([
            TestBooking("4", "Orange", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Orange", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "Orange", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "Orange", avStatus: "Orphaned", creationOrder: 7),
        ]);

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState2 =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        resultingAvailabilityState2.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1");

        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "2");
        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "3");
        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "5");
        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "6");
        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "7");
    }

    /// <summary>
    ///     Prove that having the capacity across multiple sessions in any combination gives you the same outcome
    /// </summary>
    [Fact]
    public async Task TheBestFitProblem_8()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Blue", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Blue", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Blue", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "Green", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "Blue", avStatus: "Orphaned", creationOrder: 7), //should fail
            TestBooking("8", "Green", avStatus: "Orphaned", creationOrder: 8),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 6), //single session with capcity 6
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 2),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState1 =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        resultingAvailabilityState1.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5", "6", "7", "8");
        resultingAvailabilityState1.AvailableSlots.Should().BeEmpty();


        //third pass
        sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue"],
                capacity: 2), //same capacity 6 divided by 3 sessions of length 2
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 2),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 2),
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 2),
        };

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState3 =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        resultingAvailabilityState1.Recalculations.Should().BeEquivalentTo(resultingAvailabilityState3.Recalculations);
        resultingAvailabilityState1.AvailableSlots.Should().BeEquivalentTo(resultingAvailabilityState3.AvailableSlots);

        //second pass
        sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue"],
                capacity: 3), //same capacity 6 divided by 2 sessions of length 3
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 3),
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 2),
        };

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState2 =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        resultingAvailabilityState1.Recalculations.Should().BeEquivalentTo(resultingAvailabilityState2.Recalculations);
        resultingAvailabilityState1.AvailableSlots.Should().BeEquivalentTo(resultingAvailabilityState2.AvailableSlots);
    }
}
