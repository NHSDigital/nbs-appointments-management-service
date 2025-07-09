namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class HasAvailabilityTests : BookingAvailabilityStateServiceTestBase
{
    /// <summary>
    /// There exist slots in the daterange with no bookings in, therefore must support the service
    /// </summary>
    [Fact]
    public async Task NoSessionsForService()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 1),
        };

        SetupHasAvailabilityData(bookings, []);

        var hasAvailability = await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));

        Assert.False(hasAvailability);
    }
    
    /// <summary>
    /// There exist slots in the daterange with no bookings in, therefore must support the service
    /// </summary>
    [Fact]
    public async Task EmptySlotExists_1()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 3),
            TestBooking("4", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 5),
            TestBooking("6", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 6),
            TestBooking("7", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 7),
            TestBooking("8", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 8),
            TestBooking("9", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 9),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green", "Blue"], capacity: 2),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green"], capacity: 1),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Blue"], capacity: 1),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Blue"], capacity: 1),
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasAvailability = await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));

        Assert.True(hasAvailability);
    }
    
    /// <summary>
    /// Empty slots for different services does not return true
    /// </summary>
    [Fact]
    public async Task EmptySlotExists_2()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 3),
            TestBooking("4", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 5),
            TestBooking("6", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 6),
            TestBooking("7", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 7),
            TestBooking("8", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 8),
            TestBooking("9", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 9),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:20", ["Blue"], capacity: 5),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Green"], capacity: 4),
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasGreenAvailability = await Sut.HasAvailability("Green", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.False(hasGreenAvailability);
        
        var hasBlueAvailability = await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasBlueAvailability);
    }
    
    /// <summary>
    /// Despite there being no empty slots remaining, there is still guaranteed capacity in the available slot
    /// </summary>
    [Fact]
    public async Task CapacityRemaining_1()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 3),
            TestBooking("4", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 5),
            TestBooking("6", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 6),
            TestBooking("7", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 7),
            TestBooking("8", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 8),
            TestBooking("9", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 9),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Green", "Blue"], capacity: 10)
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasBlueAvailability = await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasBlueAvailability);
        
        var hasGreenAvailability = await Sut.HasAvailability("Green", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasGreenAvailability);
    }
    
    /// <summary>
    /// Bookings for different services at the same slot time don't change the outcome
    /// </summary>
    [Fact]
    public async Task CapacityRemaining_2()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 3),
            TestBooking("4", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 5),
            TestBooking("6", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 6),
            TestBooking("7", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 7),
            TestBooking("8", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 8),
            TestBooking("9", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 9),
            TestBooking("10", "Purple", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 10),
            TestBooking("11", "Purple", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 11),
            TestBooking("12", "Purple", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 12),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Green", "Blue"], capacity: 10),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Purple"], capacity: 3),
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasBlueAvailability = await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasBlueAvailability);
        
        var hasPurpleAvailability = await Sut.HasAvailability("Purple", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.False(hasPurpleAvailability);
    }
    
    /// <summary>
    /// Equivalent slots with same supported services are grouped
    /// </summary>
    [Fact]
    public async Task CapacityRemaining_3()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 3),
            TestBooking("4", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 5),
            TestBooking("6", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 6),
            TestBooking("7", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 7),
            TestBooking("8", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 8),
            TestBooking("9", "Green", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 9),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Green", "Blue"], capacity: 5),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Green", "Blue"], capacity: 3),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Blue", "Green"], capacity: 2),
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasAvailability = await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));

        Assert.True(hasAvailability);
    }
    
    /// <summary>
    /// Due to ordering the session data descending, it should result in quicker execution
    /// </summary>
    [Fact]
    public async Task CapacityRemaining_ReverseOrderingCanExecuteQuicker()
    {
        var bookings = new List<Booking>();

        //all 18 blue bookings are in the slots
        var startTime = new TimeOnly(09, 00);

        for (var i = 0; i < 18; i++)
        {
            bookings.Add(TestBooking($"{i}", "Blue", new DateOnly(2025, 1, 6), from: $"{startTime.ToString("HH:mm")}", avStatus: "Supported", creationOrder: i));
            startTime = startTime.AddMinutes(10);
        }
        
        startTime = new TimeOnly(09, 00);
        
        //only 17/18 green bookings are in the slots, meaning the 11:50-12:00 slot has one spare capacity
        for (var i = 0; i < 17; i++)
        {
            bookings.Add(TestBooking($"{i}", "Green", new DateOnly(2025, 1, 6), from: $"{startTime.ToString("HH:mm")}", avStatus: "Supported", creationOrder: i));
            startTime = startTime.AddMinutes(10);
        }
        
        var sessions = new List<SessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "12:00", ["Blue", "Green"], capacity: 2),
        };

        SetupHasAvailabilityData(bookings, sessions);

        //it should find the 11:50-12:00 slot having capacity first
        
        var hasBlueAvailability = await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasBlueAvailability);
        var hasGreenAvailability = await Sut.HasAvailability("Green", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasGreenAvailability);
    }
    
    /// <summary>
    /// Due to ordering the session slot data descending, it should result in quicker execution
    /// </summary>
    [Fact]
    public async Task EmptySlotExists_ReverseOrderingCanExecuteQuicker()
    {
        var bookings = new List<Booking>();

        var startTime = new TimeOnly(9, 0);
        
        //set up the data so that ONLY 11:50-12:00 is free, 17/18 of the other slots are taken
        //it should find the 11:50 slot being empty first
        for (var i = 0; i < 17; i++)
        {
            bookings.Add(TestBooking($"{i}", "Blue", new DateOnly(2025, 1, 6), from: $"{startTime.ToString("HH:mm")}", avStatus: "Supported", creationOrder: i));
            startTime = startTime.AddMinutes(10);
        }

        var sessions = new List<SessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "12:00", ["Blue"], capacity: 1),
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasAvailability = await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));

        Assert.True(hasAvailability);
    }
}
