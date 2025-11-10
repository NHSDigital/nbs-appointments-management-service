namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class SessionProposalTests : BookingAvailabilityStateServiceTestBase
{
    [Fact]
    public async Task AvailabilityChangeProposal_EditSession_SingleDay()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["Green", "Blue"],
            SlotLength = 10,
            Capacity = 1
        };
        var replacement = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["Green"],
            SlotLength = 10,
            Capacity = 1
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 1, 9, 10, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Supported", creationOrder: 3)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1),
        };
        SetupAvailabilityAndBookings(bookings, sessions);
        var expectedReallocatedBookings = 1;
        var expectedUnaccommodatedBookings = 2;

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, replacement);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedReallocatedBookings);
        proposalMetrics.NewlyOrphanedBookingsCount.Should().Be(expectedUnaccommodatedBookings);
    }

    [Fact]
    public async Task AvailabilityChangeProposal_EditSession_MatchingSessionNotFound()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 30, 0),
            Until = new TimeOnly(10, 0, 0),
            Services = ["Green", "Blue"],
            SlotLength = 10,
            Capacity = 1
        };
        var replacement = new Session
        {
            From = new TimeOnly(9, 0, 0),
            Until = new TimeOnly(10, 0, 0),
            Services = ["Green"],
            SlotLength = 10,
            Capacity = 1
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 1, 9, 10, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Supported", creationOrder: 3)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1),
        };
        SetupAvailabilityAndBookings(bookings, sessions);

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, replacement);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.MatchingSessionNotFound.Should().BeTrue();
    }

    [Fact]
    public async Task AvailabilityChangeProposal_EditSession_MultipleDays()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["Green", "Blue"],
            SlotLength = 10,
            Capacity = 1
        };
        var replacement = new Session
        {
            From = new TimeOnly(9, 0, 0),
            Until = new TimeOnly(10, 0, 0),
            Services = ["Green"],
            SlotLength = 10,
            Capacity = 1
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 3, 17, 0, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Orange", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Supported", creationOrder: 3),
            TestBooking("1", "Blue", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 4),
            TestBooking("2", "Orange", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 5),
            TestBooking("3", "Blue", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 6),
            TestBooking("1", "Blue", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 7),
            TestBooking("2", "Orange", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 8),
            TestBooking("3", "Blue", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 9)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0), ["Green", "Blue"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 2, 9, 0, 0), new DateTime(2025, 1, 2, 10, 0, 0), ["Green", "Blue"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 2, 9, 0, 0), new DateTime(2025, 1, 2, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 3, 9, 0, 0), new DateTime(2025, 1, 3, 10, 0, 0), ["Green", "Blue"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 3, 9, 0, 0), new DateTime(2025, 1, 3, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
        };
        SetupAvailabilityAndBookings(bookings, sessions);
        var expectedReallocatedBookings = 3;
        var expectedUnaccommodatedBookings = 6;

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, replacement);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedReallocatedBookings);
        proposalMetrics.NewlyOrphanedBookingsCount.Should().Be(expectedUnaccommodatedBookings);
    }

    [Fact]
    public async Task AvailabilityChangeProposal_CancelSingleSession_MatchingSessionNotFound()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 30, 0),
            Until = new TimeOnly(10, 0, 0),
            Services = ["Green", "Blue"],
            SlotLength = 10,
            Capacity = 1
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 1, 9, 10, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Supported", creationOrder: 3)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1),
        };
        SetupAvailabilityAndBookings(bookings, sessions);

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, null);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.MatchingSessionNotFound.Should().BeTrue();
    }

    [Fact]
    public async Task AvailabilityChangeProposal_CancelSingleSession_SingleDay()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["Green", "Blue"],
            SlotLength = 10,
            Capacity = 1
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 1, 9, 10, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Supported", creationOrder: 3)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1),
        };
        SetupAvailabilityAndBookings(bookings, sessions);
        var expectedReallocatedBookings = 1;
        var expectedUnaccommodatedBookings = 2;

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, null);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedReallocatedBookings);
        proposalMetrics.NewlyOrphanedBookingsCount.Should().Be(expectedUnaccommodatedBookings);
    }

    [Fact]
    public async Task AvailabilityChangeProposal_CancelSingleSession_MultipleDays()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["Green", "Blue"],
            SlotLength = 10,
            Capacity = 1
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 3, 17, 0, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Orange", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Supported", creationOrder: 3),
            TestBooking("4", "Blue", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "Orange", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 5),
            TestBooking("6", "Blue", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 6),
            TestBooking("7", "Blue", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 7),
            TestBooking("8", "Orange", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 8),
            TestBooking("9", "Blue", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 9)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0), ["Green", "Blue"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 2, 9, 0, 0), new DateTime(2025, 1, 2, 10, 0, 0), ["Green", "Blue"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 2, 9, 0, 0), new DateTime(2025, 1, 2, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 3, 9, 0, 0), new DateTime(2025, 1, 3, 10, 0, 0), ["Green", "Blue"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 3, 9, 0, 0), new DateTime(2025, 1, 3, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
        };
        SetupAvailabilityAndBookings(bookings, sessions);
        var expectedReallocatedBookings = 3;
        var expectedUnaccommodatedBookings = 6;

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, null);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedReallocatedBookings);
        proposalMetrics.NewlyOrphanedBookingsCount.Should().Be(expectedUnaccommodatedBookings);
    }

    [Fact]
    public async Task AvailabilityChangeProposal_CancelAllSessions_SingleDay()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["Green", "Blue"],
            SlotLength = 10,
            Capacity = 1
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 1, 9, 10, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Supported", creationOrder: 3)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1),
        };
        SetupAvailabilityAndBookings(bookings, sessions);
        var expectedReallocatedBookings = 0;
        var expectedUnaccommodatedBookings = 3;

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, null, true);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedReallocatedBookings);
        proposalMetrics.NewlyOrphanedBookingsCount.Should().Be(expectedUnaccommodatedBookings);
    }

    [Fact]
    public async Task AvailabilityChangeProposal_CancelAllSessions_MultipleDays()
    {
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 3, 17, 0, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Orange", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Supported", creationOrder: 3),
            TestBooking("1", "Blue", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 4),
            TestBooking("2", "Orange", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 5),
            TestBooking("3", "Blue", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 6),
            TestBooking("1", "Blue", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 7),
            TestBooking("2", "Orange", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 8),
            TestBooking("3", "Blue", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 9)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0), ["Green", "Blue"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 2, 9, 0, 0), new DateTime(2025, 1, 2, 10, 0, 0), ["Green", "Blue"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 2, 9, 0, 0), new DateTime(2025, 1, 2, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 3, 9, 0, 0), new DateTime(2025, 1, 3, 10, 0, 0), ["Green", "Blue"],
                capacity: 1),
            TestSession(new DateTime(2025, 1, 3, 9, 0, 0), new DateTime(2025, 1, 3, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
        };
        SetupAvailabilityAndBookings(bookings, sessions);
        var expectedReallocatedBookings = 0;
        var expectedUnaccommodatedBookings = 9;

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, null, null, true);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedReallocatedBookings);
        proposalMetrics.NewlyOrphanedBookingsCount.Should().Be(expectedUnaccommodatedBookings);
    }

    [Fact]
    public async Task AvailabilityChangeProposal_MatchingSessionNotFound()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 30, 0),
            Until = new TimeOnly(10, 0, 0),
            Services = ["Green", "Blue"],
            SlotLength = 10,
            Capacity = 1
        };
        var replacement = new Session
        {
            From = new TimeOnly(9, 0, 0),
            Until = new TimeOnly(10, 0, 0),
            Services = ["Green"],
            SlotLength = 10,
            Capacity = 1
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 1, 9, 10, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Supported", creationOrder: 3)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1),
        };
        SetupAvailabilityAndBookings(bookings, sessions);

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, replacement);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.MatchingSessionNotFound.Should().BeTrue();
    }
}
