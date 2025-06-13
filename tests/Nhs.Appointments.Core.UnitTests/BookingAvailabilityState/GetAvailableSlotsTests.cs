namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class GetAvailableSlotsTests : BookingAvailabilityStateServiceTestBase
{
    [Fact]
    public async Task MakesNoChangesIfAllAppointmentsAreStillValid()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", "09:10", avStatus: "Supported"),
            TestBooking("2", "Green", "09:20", avStatus: "Supported"),
            TestBooking("3", "Green", "09:30", avStatus: "Supported")
        };

        var sessions = new List<LinkedSessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var slots = await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 30, 0));
        slots.Should().HaveCount(15);
    }

    [Fact]
    public async Task SchedulesOrphanedAppointmentsIfPossible()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", "09:10"),
            TestBooking("2", "Green", "09:20"),
            TestBooking("3", "Green", "09:30")
        };

        var sessions = new List<LinkedSessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var slots = await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 30, 0));
        slots.Should().HaveCount(15);
    }

    [Fact]
    public async Task OrphansLiveAppointmentsIfTheyCannotBeFulfilled()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", "09:10", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Green", "09:10", avStatus: "Supported", creationOrder: 2)
        };

        var sessions = new List<LinkedSessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var slots = await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 30, 0));
        slots.Should().HaveCount(17);
    }

    [Fact]
    public async Task DeletesProvisionalAppointments()
    {
        var utcNow = DateTime.UtcNow;
        base.TimeProvider.Setup(x => x.GetUtcNow()).Returns(utcNow);
        
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", avStatus: "Supported", status: "Booked", creationOrder: 1),
            TestBooking("2", "Green", "09:10", status: "Provisional", creationOrder: 2, creationDate: utcNow.AddMinutes(-3))
        };

        var sessions = new List<LinkedSessionInstance> { TestSession("10:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var slots = await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0));
        slots.Should().HaveCount(12);
    }

    [Fact]
    public async Task ExpiredProvisionalAppointments_NotDeleted()
    {
        var utcNow = DateTime.UtcNow;
        base.TimeProvider.Setup(x => x.GetUtcNow()).Returns(utcNow);
        
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", avStatus: "Supported", status: "Booked", creationOrder: 1),
            TestBooking("2", "Green", "09:10", status: "Provisional", creationOrder: 2, creationDate: utcNow.AddMinutes(-8))
        };

        var sessions = new List<LinkedSessionInstance> { TestSession("10:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var slots = await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0));
        slots.Should().HaveCount(12);
    }
    
    [Fact]
    public async Task PrioritisesAppointmentsByCreatedDate()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", "09:30", creationOrder: 3),
            TestBooking("2", "Green", "09:30", creationOrder: 1),
            TestBooking("3", "Green", "09:30", creationOrder: 2)
        };

        var sessions = new List<LinkedSessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var slots = await Sut.GetAvailableSlots(MockSite, new DateTime(2025, 1, 1, 9, 30, 0), new DateTime(2025, 1, 1, 9, 30, 0));
        slots.Should().HaveCount(17);
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

        var sessions = new List<LinkedSessionInstance>
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

        var sessions = new List<LinkedSessionInstance>
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
    
    [Fact]
    public async Task MultipleServices_GreedyAllocationWhenAllSessionsEquivalent()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1)
        };

        var sessions = new List<LinkedSessionInstance>
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

        var sessions = new List<LinkedSessionInstance>
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

        var sessions = new List<LinkedSessionInstance>
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

        var sessions = new List<LinkedSessionInstance>
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

        var sessions = new List<LinkedSessionInstance>
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
}
