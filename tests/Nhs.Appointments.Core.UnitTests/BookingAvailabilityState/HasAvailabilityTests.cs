using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class HasAvailabilityTests : BookingAvailabilityStateServiceTestBase
{
    private const string SuccessLogMessage = "HasAvailability short circuit success";
    private const string UnsuccessfulLogMessage = "HasAvailability short circuit attempt unsuccessful";

    /// <summary>
    ///     There exist no sessions in the date-range for the queried service, therefore cannot support the service
    /// </summary>
    [Fact]
    public async Task NoSessionsForService()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateOnly(2025, 1, 6), avStatus: "Supported", creationOrder: 1),
        };

        SetupHasAvailabilityData(bookings, []);

        var hasAvailability =
            await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.False(hasAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - No sessions for service: 'Blue', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    
    /// <summary>
    /// When none of the short-circuit checks return, have to default to using the build state approach that uses allocation algorithm
    /// </summary>
    [Fact]
    public async Task FallbackToBuildState_1()
    {
        var bookings = new List<Booking>();
        var startTime = new TimeOnly(9, 0);
        
        //add 6 green bookings in from 9am-10am
        for (var i = 0; i < 6; i++)
        {
            bookings.Add(TestBooking($"{i}", "Green", new DateOnly(2025, 1, 6), from: $"{startTime.ToString("HH:mm")}",
                avStatus: "Supported", creationOrder: i));
            startTime = startTime.AddMinutes(10);
        }

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green", "Blue"], capacity: 1),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green"], capacity: 1),
        };

        SetupHasAvailabilityData(bookings, sessions);
        SetupAvailabilityAndBookings(bookings, sessions);

        //blue has availability due to the greedy allocation assigning green bookings to the second session
        var hasBlueAvailability =
            await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasBlueAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{UnsuccessfulLogMessage} - falling back to building the full state to ascertain the availability for service: 'Blue', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        
        //green has availability but still has to use allocation to confirm
        var hasGreenAvailability =
            await Sut.HasAvailability("Green", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasGreenAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{UnsuccessfulLogMessage} - falling back to building the full state to ascertain the availability for service: 'Green', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Prove greedy model inefficient behaviour still occurs
    /// </summary>
    [Fact]
    public async Task FallbackToBuildState_2()
    {
        var bookings = new List<Booking>();
        var startTime = new TimeOnly(9, 0);
        
        //add 6 green bookings in from 9am-10am
        for (var i = 0; i < 6; i++)
        {
            bookings.Add(TestBooking($"{i}", "Green", new DateOnly(2025, 1, 6), from: $"{startTime.ToString("HH:mm")}",
                avStatus: "Supported", creationOrder: i));
            startTime = startTime.AddMinutes(10);
        }

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green", "Lilac", "Purple"], capacity: 1),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green", "Blue"], capacity: 1),
        };

        SetupHasAvailabilityData(bookings, sessions);
        SetupAvailabilityAndBookings(bookings, sessions);

        //blue has no availability due to the greedy allocation assigning green bookings to the second session
        var hasBlueAvailability =
            await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.False(hasBlueAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{UnsuccessfulLogMessage} - falling back to building the full state to ascertain the availability for service: 'Blue', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        
        //green has availability but still has to use allocation to confirm
        var hasGreenAvailability =
            await Sut.HasAvailability("Green", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasGreenAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{UnsuccessfulLogMessage} - falling back to building the full state to ascertain the availability for service: 'Green', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    
    /// <summary>
    ///     There exist slots in the date-range with no bookings in, therefore must support the service
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

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green", "Blue"], capacity: 2),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green"], capacity: 1),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Blue"], capacity: 1),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Blue"], capacity: 1),
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasAvailability =
            await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Empty slot exists for service: 'Blue', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    ///     Empty slots for different services
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

        //blue has a slot with no bookings in at 9:10-9:20
        //green has no 'slots with no bookings' in
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:20", ["Blue"], capacity: 5),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Green"], capacity: 4),
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasGreenAvailability =
            await Sut.HasAvailability("Green", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.False(hasGreenAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Empty slot exists for service: 'Green', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);

        var hasBlueAvailability =
            await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasBlueAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Empty slot exists for service: 'Blue', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    ///     Despite there being no empty slots remaining, there is still guaranteed capacity in the available slot
    /// </summary>
    [Fact]
    public async Task GuaranteedCapacityRemaining_1()
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

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Green", "Blue"], capacity: 10)
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasBlueAvailability =
            await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasBlueAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Blue', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        var hasGreenAvailability =
            await Sut.HasAvailability("Green", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasGreenAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Green', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    ///     Bookings for different services at the same slot time don't change the outcome
    /// </summary>
    [Fact]
    public async Task GuaranteedCapacityRemaining_2()
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

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Green", "Blue"], capacity: 10),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Purple"], capacity: 3),
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasBlueAvailability =
            await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasBlueAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Blue', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        var hasPurpleAvailability =
            await Sut.HasAvailability("Purple", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.False(hasPurpleAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Purple', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{UnsuccessfulLogMessage} - falling back to building the full state to ascertain the availability for service: 'Purple', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    ///     Equivalent slots with same supported services are grouped
    /// </summary>
    [Fact]
    public async Task GuaranteedCapacityRemaining_3()
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

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Green", "Blue"], capacity: 5),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Green", "Blue"], capacity: 3),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "09:10", ["Blue", "Green"], capacity: 2),
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasAvailability =
            await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Blue', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    ///     Due to ordering the session data descending, it should result in quicker execution
    /// </summary>
    [Fact]
    public async Task GuaranteedCapacityRemaining_ReverseOrderingCanExecuteQuicker()
    {
        var bookings = new List<Booking>();

        //all 18 blue bookings are in the slots
        var startTime = new TimeOnly(09, 00);

        for (var i = 0; i < 18; i++)
        {
            bookings.Add(TestBooking($"{i}", "Blue", new DateOnly(2025, 1, 6), from: $"{startTime.ToString("HH:mm")}",
                avStatus: "Supported", creationOrder: i));
            startTime = startTime.AddMinutes(10);
        }

        startTime = new TimeOnly(09, 00);

        //only 17/18 green bookings are in the slots, meaning the 11:50-12:00 slot has one spare capacity
        for (var i = 0; i < 17; i++)
        {
            bookings.Add(TestBooking($"{i}", "Green", new DateOnly(2025, 1, 6), from: $"{startTime.ToString("HH:mm")}",
                avStatus: "Supported", creationOrder: i));
            startTime = startTime.AddMinutes(10);
        }

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "12:00", ["Blue", "Green"], capacity: 2),
        };

        SetupHasAvailabilityData(bookings, sessions);

        //it should find the 11:50-12:00 slot having capacity first

        var hasBlueAvailability =
            await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasBlueAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Blue', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        var hasGreenAvailability =
            await Sut.HasAvailability("Green", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasGreenAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Green', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    ///     Due to ordering the session slot data descending, it should result in quicker execution
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
            bookings.Add(TestBooking($"{i}", "Blue", new DateOnly(2025, 1, 6), from: $"{startTime.ToString("HH:mm")}",
                avStatus: "Supported", creationOrder: i));
            startTime = startTime.AddMinutes(10);
        }

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "12:00", ["Blue"], capacity: 1),
        };

        SetupHasAvailabilityData(bookings, sessions);

        var hasAvailability =
            await Sut.HasAvailability("Blue", MockSite, new DateTime(2025, 1, 6), new DateTime(2025, 1, 7));
        Assert.True(hasAvailability);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Empty slot exists for service: 'Blue', site : 'some-site', from : '01/06/2025 00:00:00', to : '01/07/2025 00:00:00'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
