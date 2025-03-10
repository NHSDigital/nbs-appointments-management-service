﻿namespace Nhs.Appointments.Core.UnitTests.AvailabilityCalculations;

public class MultiServiceTests : AvailabilityCalculationsBase
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MultipleServicesByCreatedDate(bool useV2)
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

        AvailabilityState resultingAvailabilityState;

        if (useV2)
        {
            resultingAvailabilityState = await _sut.GetAvailabilityStateV2(MockSite, new DateOnly(2025, 1, 1));
        }
        else
        {
            resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));
        }

        // Bookings 1, 2, 3, 6 and 7 should be supported
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "6", "7");

        // Bookings 4 and 5 should not be, because they were created after 6 and 7
        resultingAvailabilityState.Recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        resultingAvailabilityState.Recalculations.Should().NotContain(r => r.Booking.Reference == "5");

        resultingAvailabilityState.Bookings.Should().HaveCount(5);
        resultingAvailabilityState.Bookings.Select(b => b.Reference).Should().BeEquivalentTo("1", "2", "3", "6", "7");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TheBestFitProblem(bool useV2)
    {
        // See: https://app.mural.co/t/nhsdigital8118/m/nhsdigital8118/1737058837342/35b45e0418f8661f6ad19efed4bf3fd0cc9bb3d5

        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1),
            TestSession("09:00", "10:00", ["Orange", "Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        AvailabilityState resultingAvailabilityState;

        if (useV2)
        {
            resultingAvailabilityState = await _sut.GetAvailabilityStateV2(MockSite, new DateOnly(2025, 1, 1));
        }
        else
        {
            resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));
        }

        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3");

        resultingAvailabilityState.Bookings.Should().HaveCount(3);
        resultingAvailabilityState.Bookings.Select(b => b.Reference).Should().BeEquivalentTo("1", "2", "3");
    }
    
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TheBestFitProblem_2(bool useV2)
    {
        // See: https://app.mural.co/t/nhsdigital8118/m/nhsdigital8118/1737058837342/35b45e0418f8661f6ad19efed4bf3fd0cc9bb3d5

        var bookings = new List<Booking>
        {
            TestBooking("1", "B", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "B", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "A", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "A", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "B", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "C", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "D", avStatus: "Orphaned", creationOrder: 7),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["A", "B", "C"], capacity: 1),
            TestSession("09:00", "10:00", ["A", "C", "D"], capacity: 2),
            TestSession("09:00", "10:00", ["B", "C", "D"], capacity: 2),
            TestSession("09:00", "10:00", ["A", "B", "D"], capacity: 2),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        AvailabilityState resultingAvailabilityState;

        if (useV2)
        {
            resultingAvailabilityState = await _sut.GetAvailabilityStateV2(MockSite, new DateOnly(2025, 1, 1));
        }
        else
        {
            resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));
        }

        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5", "6", "7");

        resultingAvailabilityState.Bookings.Should().HaveCount(7);
        resultingAvailabilityState.Bookings.Select(b => b.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5", "6", "7");
    }
    
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TheBestFitProblem_3(bool useV2)
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "B", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "A", avStatus: "Orphaned", creationOrder: 2)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["A", "B"], capacity: 1),
            TestSession("09:00", "10:00", ["B", "C", "D"], capacity: 4)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        AvailabilityState resultingAvailabilityState;

        if (useV2)
        {
            resultingAvailabilityState = await _sut.GetAvailabilityStateV2(MockSite, new DateOnly(2025, 1, 1));
        }
        else
        {
            resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));
        }

        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2");

        resultingAvailabilityState.Bookings.Should().HaveCount(7);
        resultingAvailabilityState.Bookings.Select(b => b.Reference).Should().BeEquivalentTo("1", "2");
    }
}
