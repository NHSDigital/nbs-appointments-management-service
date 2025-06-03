namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class BuildRecalculationsTests : BookingAvailabilityStateServiceTestBase
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
            
        var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        // Bookings 1, 2, 3, 6 and 7 should be supported
        recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "6", "7");

        // Bookings 4 and 5 should not be, because they were created after 6 and 7
        recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        recalculations.Should().NotContain(r => r.Booking.Reference == "5");

        recalculations.Should().HaveCount(5);
        recalculations.Select(b => b.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "6", "7");
    }

    /// <summary>
    /// Prove that greedy model can in some cases increase utilisation where the SingleService code would not
    /// </summary>
    [Fact]
    public async Task MultipleServices_IncreasedUtilisation_1()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "C", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "C", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "C", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "F", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "G", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "E", avStatus: "Orphaned", creationOrder: 6),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["B", "C", "D", "E", "F", "G"], capacity: 3),
            TestSession("09:00", "09:10", ["A", "C", "D"], capacity: 3),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        //prove all 6 find a slot 
        recalculations.Where(x => x.Action == AvailabilityUpdateAction.SetToSupported).Should().HaveCount(6);
    }
    
    /// <summary>
    /// Prove that greedy model can in some cases increase utilisation where the SingleService code would not
    /// </summary>
    [Fact]
    public async Task MultipleServices_IncreasedUtilisation_2()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "C", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "C", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "C", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "B", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "B", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "B", avStatus: "Orphaned", creationOrder: 6),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["B", "C", "D"], capacity: 3),
            TestSession("09:00", "09:10", ["A", "C", "D"], capacity: 3),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        //prove all 6 find a slot due to alphabetical ordering
        recalculations.Where(x => x.Action == AvailabilityUpdateAction.SetToSupported).Should().HaveCount(6);
    }
    
    /// <summary>
    /// Prove that greedy model isn't always optimal, and can lead to loss in utilisation
    /// </summary>
    [Fact]
    public async Task MultipleServices_LostUtilisation_1()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "C", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "C", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "C", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "F", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "G", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "E", avStatus: "Orphaned", creationOrder: 6),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["B", "C", "D", "E", "F", "G"], capacity: 3),
            TestSession("09:00", "09:10", ["A", "C", "D", "U", "Z", "H", "I", "J"], capacity: 3),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        // Bookings 1, 2, 3 should be supported
        recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3");

        // Bookings 4, 5, 6 should not be, due to non-optimal greedy algorithm
        recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        recalculations.Should().NotContain(r => r.Booking.Reference == "5");
        recalculations.Should().NotContain(r => r.Booking.Reference == "6");
    }
    
    /// <summary>
    /// Prove that greedy model isn't always optimal, and can lead to loss in utilisation
    /// This test proves a scenario where just selecting the first available slot would actually have been preferable over the alphabetical ordering
    /// </summary>
    [Fact]
    public async Task MultipleServices_LostUtilisation_2()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "C", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "C", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "C", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "B", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "B", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "B", avStatus: "Orphaned", creationOrder: 6),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["D", "C", "E"], capacity: 3),
            TestSession("09:00", "09:10", ["B", "C", "D"], capacity: 3),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        // Bookings 1, 2, 3 should be supported
        recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3");

        // Bookings 4, 5, 6 should not be, due to non-optimal greedy algorithm
        recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        recalculations.Should().NotContain(r => r.Booking.Reference == "5");
        recalculations.Should().NotContain(r => r.Booking.Reference == "6");
    }
    
    [Fact]
    public async Task TheBestFitProblem()
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

        var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        // Bookings 1 and 2 can be booked
        recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2");

        // Booking 3 could have been booked if we rejuggled appointments, but currently we do not
        recalculations.Select(b => b.Booking.Reference).Should().NotContain(r => r == "3");

        recalculations.Should().HaveCount(2);
        recalculations.Select(b => b.Booking.Reference).Should().BeEquivalentTo("1", "2");
    }
    [Fact]
    public async Task MultiplePassesProduceTheSameResult()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Orange"),
            TestBooking("2", "Blue", creationOrder: 2),
            TestBooking("3", "Green", creationOrder: 3),
            TestBooking("4", "Orange", creationOrder: 4),
            TestBooking("5", "Orange", creationOrder: 5),
            TestBooking("6", "Green", creationOrder: 6),
            TestBooking("7", "Green", "09:10", creationOrder: 7),
            TestBooking("8", "Blue", "09:10", creationOrder: 8),
            TestBooking("9", "Orange", "09:10", creationOrder: 9),
            TestBooking("10", "Blue", "09:30", creationOrder: 10),
            TestBooking("11", "Green", "09:30", creationOrder: 11),
            TestBooking("12", "Green", "09:30", creationOrder: 12),
            TestBooking("13", "Green", "09:30", creationOrder: 13),
            TestBooking("14", "Green", "09:40", creationOrder: 14),
            TestBooking("15", "Orange", "09:40", creationOrder: 15),
            TestBooking("16", "Blue", "09:40", creationOrder: 16),
            TestBooking("17", "Green", "09:40", creationOrder: 17),
            TestBooking("18", "Orange", "09:50", creationOrder: 18),
            TestBooking("19", "Green", "09:50", creationOrder: 19),
            TestBooking("20", "Blue", "09:50", creationOrder: 20),
            TestBooking("21", "Blue", "09:50", creationOrder: 21)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue", "Orange"], capacity: 2),
            TestSession("09:00", "09:40", ["Blue", "Green"], capacity: 1),
            TestSession("09:30", "10:00", ["Orange", "Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0))).ToList();

        recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "8", "9", "7", "11", "12", "10",
                "14", "15", "16", "19", "20", "18");

        // Bookings 4 and 5 should not be, because they were created after 6 and 7
        recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        recalculations.Should().NotContain(r => r.Booking.Reference == "5");
        recalculations.Should().NotContain(r => r.Booking.Reference == "6");
        recalculations.Should().NotContain(r => r.Booking.Reference == "13");
        recalculations.Should().NotContain(r => r.Booking.Reference == "17");
        recalculations.Should().NotContain(r => r.Booking.Reference == "21");

        var runs = 10;
        while (runs > 0)
        {
            var newResult = await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0));
            newResult.Should().BeEquivalentTo(recalculations);
            runs -= 1;
        }
    }
    
}
