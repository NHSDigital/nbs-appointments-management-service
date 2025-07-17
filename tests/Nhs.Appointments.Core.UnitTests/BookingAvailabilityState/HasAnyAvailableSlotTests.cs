using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class HasAnyAvailableSlotTests : BookingAvailabilityStateServiceTestBase
{
    private const string SuccessLogMessage = "HasAnyAvailableSlot short circuit success";
    private const string UnsuccessfulLogMessage = "HasAnyAvailableSlot short circuit attempt unsuccessful";

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

        var blue = await Sut.HasAnyAvailableSlot("Blue", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.False(blue.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - No sessions for service: 'Blue', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    ///     When none of the short-circuit checks return, have to default to using the build state approach that uses
    ///     allocation algorithm
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
        var blue = await Sut.HasAnyAvailableSlot("Blue", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(blue.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{UnsuccessfulLogMessage} - falling back to building the full state to ascertain the availability for service: 'Blue', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        //green has availability but still has to use allocation to confirm
        var green =
            await Sut.HasAnyAvailableSlot("Green", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(green.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{UnsuccessfulLogMessage} - falling back to building the full state to ascertain the availability for service: 'Green', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    ///     Prove greedy model inefficient behaviour still occurs
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
        var blue =
            await Sut.HasAnyAvailableSlot("Blue", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.False(blue.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{UnsuccessfulLogMessage} - falling back to building the full state to ascertain the availability for service: 'Blue', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        //green has availability but still has to use allocation to confirm
        var green =
            await Sut.HasAnyAvailableSlot("Green", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(green.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{UnsuccessfulLogMessage} - falling back to building the full state to ascertain the availability for service: 'Green', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
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

        var blue =
            await Sut.HasAnyAvailableSlot("Blue", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(blue.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Empty slot exists for service: 'Blue', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
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

        var green =
            await Sut.HasAnyAvailableSlot("Green", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.False(green.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Empty slot exists for service: 'Green', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);

        var blue =
            await Sut.HasAnyAvailableSlot("Blue", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(blue.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Empty slot exists for service: 'Blue', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
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

        var blue =
            await Sut.HasAnyAvailableSlot("Blue", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(blue.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Blue', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        var green =
            await Sut.HasAnyAvailableSlot("Green", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(green.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Green', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
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

        var blue =
            await Sut.HasAnyAvailableSlot("Blue", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(blue.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Blue', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        var purple =
            await Sut.HasAnyAvailableSlot("Purple", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.False(purple.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Purple', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
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
                                $"{UnsuccessfulLogMessage} - falling back to building the full state to ascertain the availability for service: 'Purple', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
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

        var blue =
            await Sut.HasAnyAvailableSlot("Blue", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(blue.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Blue', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
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

        var blue =
            await Sut.HasAnyAvailableSlot("Blue", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(blue.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Blue', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        var green =
            await Sut.HasAnyAvailableSlot("Green", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(green.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Guaranteed slot with capacity exists for service: 'Green', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
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

        var blue = await Sut.HasAnyAvailableSlot("Blue", MockSite, new DateOnly(2025, 1, 6), new DateOnly(2025, 1, 7));
        Assert.True(blue.hasSlot);

        _logger.Verify(x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()
                            .Contains(
                                $"{SuccessLogMessage} - Empty slot exists for service: 'Blue', site : 'some-site', from : '01/06/2025', to : '01/07/2025'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    
    [Fact]
    public void ReturnsSingleRange_WhenRangeIsExactly7Days()
    {
        var from = new DateOnly(2025, 7, 1);
        var to = new DateOnly(2025, 7, 7);

        var result = BookingAvailabilityStateService.GetWeekPartitions(from, to);

        Assert.Single(result);
        Assert.Equal(from, result[0].From);
        Assert.Equal(to, result[0].To);
    }

    [Fact]
    public void ReturnsSingleRange_WhenRangeIsSameDay()
    {
        var from = new DateOnly(2025, 7, 5);
        var to = new DateOnly(2025, 7, 5);

        var result = BookingAvailabilityStateService.GetWeekPartitions(from, to);

        Assert.Single(result);
        Assert.Equal(from, result[0].From);
        Assert.Equal(to, result[0].To);
    }

    [Fact]
    public void ReturnsSingleRange_WhenRangeIsLessThan7Days()
    {
        var from = new DateOnly(2025, 7, 1);
        var to = new DateOnly(2025, 7, 3);

        var result = BookingAvailabilityStateService.GetWeekPartitions(from, to);

        Assert.Single(result);
        Assert.Equal(from, result[0].From);
        Assert.Equal(to, result[0].To);
    }

    [Fact]
    public void ReturnsTwoRanges_WhenRangeIs9Days()
    {
        var from = new DateOnly(2025, 7, 1);
        var to = new DateOnly(2025, 7, 10);

        var result = BookingAvailabilityStateService.GetWeekPartitions(from, to);

        Assert.Equal(2, result.Count);
        Assert.Equal(new DateOnly(2025, 7, 1), result[0].From);
        Assert.Equal(new DateOnly(2025, 7, 7), result[0].To);

        Assert.Equal(new DateOnly(2025, 7, 8), result[1].From);
        Assert.Equal(new DateOnly(2025, 7, 10), result[1].To);
    }

    [Fact]
    public void ReturnsFourFullWeeks_WhenRangeIs28Days()
    {
        var from = new DateOnly(2025, 7, 1);
        var to = new DateOnly(2025, 7, 28);

        var result = BookingAvailabilityStateService.GetWeekPartitions(from, to);

        Assert.Equal(4, result.Count);
        for (var i = 0; i < 4; i++)
        {
            var expectedStart = from.AddDays(i * 7);
            var expectedEnd = expectedStart.AddDays(6);
            Assert.Equal(expectedStart, result[i].From);
            Assert.Equal(expectedEnd, result[i].To);
        }
    }

    [Fact]
    public void LastRangeEndsExactlyOnToDate()
    {
        var from = new DateOnly(2025, 7, 1);
        var to = new DateOnly(2025, 7, 17); // 17 days

        var result = BookingAvailabilityStateService.GetWeekPartitions(from, to);

        Assert.Equal(3, result.Count);
        Assert.Equal(new DateOnly(2025, 7, 1), result[0].From);
        Assert.Equal(new DateOnly(2025, 7, 7), result[0].To);

        Assert.Equal(new DateOnly(2025, 7, 8), result[1].From);
        Assert.Equal(new DateOnly(2025, 7, 14), result[1].To);

        Assert.Equal(new DateOnly(2025, 7, 15), result[2].From);
        Assert.Equal(new DateOnly(2025, 7, 17), result[2].To);
    }

    [Fact]
    public void ReturnsEmptyList_WhenFromIsAfterTo()
    {
        var from = new DateOnly(2025, 7, 10);
        var to = new DateOnly(2025, 7, 1);

        var result = BookingAvailabilityStateService.GetWeekPartitions(from, to);

        Assert.Empty(result);
    }
}
