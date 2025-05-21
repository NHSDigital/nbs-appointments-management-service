namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class MultiServiceTests : BookingAvailabilityStateServiceTestBase
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
    /// Show that the greedy model allocates a booking to the slot with the fewest available supported services.
    /// </summary>
    [Fact]
    public async Task MultipleServices_GreedyAllocationBySessionServiceLengthsDescending_1()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue", "Orange"], capacity: 1),
            TestSession("09:00", "09:10", ["Black", "Blue", "Lilac", "Purple"], capacity: 1),
            //prove this slot is taken
            TestSession("09:00", "09:10", ["Gold", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["White", "Blue", "Yellow"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var availableSlots = (await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        availableSlots.Count.Should().Be(3);
        
        //prove the other 3 slots with more services are still available
        Assert.Multiple(
            () => availableSlots[0].Should().BeEquivalentTo(sessions[0].ToSlots().Single()),
            () => availableSlots[1].Should().BeEquivalentTo(sessions[1].ToSlots().Single()),
            () => availableSlots[2].Should().BeEquivalentTo(sessions[3].ToSlots().Single())
        );
    }
    
    /// <summary>
    /// Show that the greedy model allocates a booking to the slot with the fewest available supported services.
    /// </summary>
    [Fact]
    public async Task MultipleServices_GreedyAllocationBySessionServiceLengthsDescending_2()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue", "Orange"], capacity: 1),
            TestSession("09:00", "09:10", ["Black", "Blue", "Lilac", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["White", "Blue", "Yellow"], capacity: 1),
            //prove this slot is taken
            TestSession("09:00", "09:10", ["Blue"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var availableSlots = (await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        availableSlots.Count.Should().Be(3);
        
        //prove the other 3 slots with more services are still available
        Assert.Multiple(
            () => availableSlots[0].Should().BeEquivalentTo(sessions[0].ToSlots().Single()),
            () => availableSlots[1].Should().BeEquivalentTo(sessions[1].ToSlots().Single()),
            () => availableSlots[2].Should().BeEquivalentTo(sessions[2].ToSlots().Single())
        );
    }
    
    /// <summary>
    /// Show that the greedy model allocates a booking to the slot with the fewest available supported services.
    /// </summary>
    [Fact]
    public async Task MultipleServices_GreedyAllocationBySessionServiceLengthsDescending_3()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1)
        };

        var sessions = new List<SessionInstance>
        {
            //prove this slot is taken
            TestSession("09:00", "09:10", ["Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue", "Orange"], capacity: 1),
            TestSession("09:00", "09:10", ["Black", "Blue", "Lilac", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["White", "Blue", "Yellow"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var availableSlots = (await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        availableSlots.Count.Should().Be(3);
        
        //prove the other 3 slots with more services are still available
        Assert.Multiple(
            () => availableSlots[0].Should().BeEquivalentTo(sessions[1].ToSlots().Single()),
            () => availableSlots[1].Should().BeEquivalentTo(sessions[2].ToSlots().Single()),
            () => availableSlots[2].Should().BeEquivalentTo(sessions[3].ToSlots().Single())
        );
    }
    
    /// <summary>
    /// Show that the greedy model allocates a booking to the slot with the fewest available supported services.
    /// If some have equal service length, it will then allocate to the slot by supported services alphabetical
    /// </summary>
    [Fact]
    public async Task MultipleServices_GreedyAllocationBySessionServicesAlphabetical_1()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue", "Orange"], capacity: 1),
            TestSession("09:00", "09:10", ["White", "Blue", "Yellow"], capacity: 1),
            //prove this slot is taken
            TestSession("09:00", "09:10", ["Blue", "Black", "Yellow"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var availableSlots = (await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        availableSlots.Count.Should().Be(2);
        
        //prove the other 2 slots with more services are still available
        Assert.Multiple(
            () => availableSlots[0].Should().BeEquivalentTo(sessions[0].ToSlots().Single()),
            () => availableSlots[1].Should().BeEquivalentTo(sessions[1].ToSlots().Single())
        );
    }
    
    /// <summary>
    /// Show that the greedy model allocates a booking to the slot with the fewest available supported services.
    /// If some have equal service length, it will then allocate to the slot by supported services alphabetical
    /// </summary>
    [Fact]
    public async Task MultipleServices_GreedyAllocationBySessionServicesAlphabetical_2()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1)
        };

        var sessions = new List<SessionInstance>
        {
            //prove this slot is taken
            TestSession("09:00", "09:10", ["Blue", "Black", "Yellow"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue", "Orange"], capacity: 1),
            TestSession("09:00", "09:10", ["White", "Blue", "Yellow"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var availableSlots = (await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        availableSlots.Count.Should().Be(2);
        
        //prove the other 2 slots with more services are still available
        Assert.Multiple(
            () => availableSlots[0].Should().BeEquivalentTo(sessions[1].ToSlots().Single()),
            () => availableSlots[1].Should().BeEquivalentTo(sessions[2].ToSlots().Single())
        );
    }
    
    /// <summary>
    /// Show that the greedy model allocates a booking to the slot with the fewest available supported services.
    /// If some have equal service length, it will then allocate to the slot by supported services alphabetical
    /// </summary>
    [Fact]
    public async Task MultipleServices_GreedyAllocationBySessionServicesAlphabetical_3()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Blue", "Black", "Yellow"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue", "Orange"], capacity: 1),
            //prove this slot is taken despite being alphabetically lowest (fewer services order is first)
            TestSession("09:00", "09:10", ["White", "Blue"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var availableSlots = (await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        availableSlots.Count.Should().Be(2);
        
        //prove the other 2 slots with more services are still available
        Assert.Multiple(
            () => availableSlots[0].Should().BeEquivalentTo(sessions[0].ToSlots().Single()),
            () => availableSlots[1].Should().BeEquivalentTo(sessions[1].ToSlots().Single())
        );
    }
    
    [Fact]
    public async Task MultipleServices_GreedyAllocationWhenAllSessionsEquivalent()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1)
        };

        var sessions = new List<SessionInstance>
        {
            //prove this slot is taken, will just use the first in the list when all else equal
            TestSession("09:00", "09:10", ["Blue", "Black", "Yellow"], capacity: 1),
            TestSession("09:00", "09:10", ["Yellow", "Black", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Black", "Yellow", "Blue"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var availableSlots = (await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        availableSlots.Count.Should().Be(2);
        
        Assert.Multiple(
            () => availableSlots[0].Services.Should().BeSameAs(sessions[1].Services),
            () => availableSlots[1].Services.Should().BeSameAs(sessions[2].Services)
        );
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
}
