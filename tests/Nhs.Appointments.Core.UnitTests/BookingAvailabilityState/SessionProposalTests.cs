using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;

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
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1), //action reduces down to just Green service
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1),
        };
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var expectedNewlySupportedBookings = 0;
        var expectedNewlyUnsupportedBookings = 1; //Blue1 no longer supported

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, replacement);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedNewlySupportedBookings);
        proposalMetrics.NewlyUnsupportedBookingsCount.Should().Be(expectedNewlyUnsupportedBookings);
    }
    
    /// <summary>
    /// A test to prove that with Greedy model allocation, the metrics show orphaned bookings when a BestFit solution would have no orphaned bookings
    /// </summary>
    [Fact]
    public async Task AvailabilityChangeProposal_EditSession_GreedyInefficiency_Alphabetical()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["A", "B", "D", "E", "F"],
            SlotLength = 10,
            Capacity = 3
        };
        var replacement = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["A", "B", "D", "E"],
            SlotLength = 10,
            Capacity = 3
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 1, 9, 10, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "A", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "A", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "A", avStatus: "Supported", creationOrder: 3),
            TestBooking("4", "D", avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "D", avStatus: "Supported", creationOrder: 5),
            TestBooking("6", "D", avStatus: "Supported", creationOrder: 6),
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["A", "B", "C", "E", "F"], capacity: 3),
            TestSession("09:00", "10:00", ["A", "B", "D", "E", "F"], capacity: 3),  //action removes "F" service
        };
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var expectedNewlySupportedBookings = 0;
        var expectedNewlyUnsupportedBookings = 3; //the three D bookings no longer have a home due to inefficient greedy model

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, replacement);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedNewlySupportedBookings);
        proposalMetrics.NewlyUnsupportedBookingsCount.Should().Be(expectedNewlyUnsupportedBookings);
    }
    
     /// <summary>
    /// A test to prove that with Greedy model allocation, the metrics show orphaned bookings when a BestFit solution would have no orphaned bookings
    /// </summary>
    [Fact]
    public async Task AvailabilityChangeProposal_EditSession_GreedyInefficiency_ServiceLength()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["A", "AB", "B", "D", "E", "F"],
            SlotLength = 10,
            Capacity = 3
        };
        var replacement = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["A", "D"],
            SlotLength = 10,
            Capacity = 3
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 1, 9, 10, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "A", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "A", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "A", avStatus: "Supported", creationOrder: 3),
            TestBooking("4", "D", avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "D", avStatus: "Supported", creationOrder: 5),
            TestBooking("6", "D", avStatus: "Supported", creationOrder: 6),
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["A", "B", "C", "E"], capacity: 3),
            TestSession("09:00", "10:00", ["A", "AB", "B", "D", "E", "F"], capacity: 3),  //action removes "AB, B, E and F" services
        };
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var expectedNewlySupportedBookings = 0;
        var expectedNewlyUnsupportedBookings = 3; //the three D bookings no longer have a home due to inefficient greedy model

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, replacement);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedNewlySupportedBookings);
        proposalMetrics.NewlyUnsupportedBookingsCount.Should().Be(expectedNewlyUnsupportedBookings);
    }
     
    /// <summary>
    /// A test that shows the same number of supported and orphaned bookings exists, but the metrics include all changes
    /// </summary>
    [Fact]
    public async Task AvailabilityChangeProposal_EditSession_SupportedBookingsSwap()
    {
        var matcher = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["A", "B"],
            SlotLength = 10,
            Capacity = 3
        };
        var replacement = new Session
        {
            From = new TimeOnly(9, 0),
            Until = new TimeOnly(10, 0),
            Services = ["B"],
            SlotLength = 10,
            Capacity = 3
        };
        var from = new DateTime(2025, 1, 1, 9, 0, 0);
        var to = new DateTime(2025, 1, 1, 9, 10, 0);
        var bookings = new List<Booking>
        {
            TestBooking("1", "A", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "A", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "A", avStatus: "Supported", creationOrder: 3),
            TestBooking("4", "B", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "B", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "B", avStatus: "Orphaned", creationOrder: 6),
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["A", "B"], capacity: 3), // action reduces to just "B" service
        };
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var expectedNewlySupportedBookings = 3; // the three B bookings are now supported
        var expectedNewlyUnsupportedBookings = 3; // the three A bookings are now NOT supported

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, replacement);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedNewlySupportedBookings);
        proposalMetrics.NewlyUnsupportedBookingsCount.Should().Be(expectedNewlyUnsupportedBookings);
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
            TestBooking("3", "Blue", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Orphaned", creationOrder: 3),
            
            TestBooking("4", "Blue", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "Orange", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 5),
            TestBooking("6", "Blue", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Orphaned", creationOrder: 6),
            
            TestBooking("7", "Blue", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 7),
            TestBooking("8", "Orange", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 8),
            TestBooking("9", "Blue", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Orphaned", creationOrder: 9)
        };
        
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0), ["Green", "Blue"],
                capacity: 1), // to be edited to just support green
            TestSession(new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
            
            TestSession(new DateTime(2025, 1, 2, 9, 0, 0), new DateTime(2025, 1, 2, 10, 0, 0), ["Green", "Blue"],
                capacity: 1), // to be edited to just support green
            TestSession(new DateTime(2025, 1, 2, 9, 0, 0), new DateTime(2025, 1, 2, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
            
            TestSession(new DateTime(2025, 1, 3, 9, 0, 0), new DateTime(2025, 1, 3, 10, 0, 0), ["Green", "Blue"],
                capacity: 1), // to be edited to just support green
            TestSession(new DateTime(2025, 1, 3, 9, 0, 0), new DateTime(2025, 1, 3, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
        };
        
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var expectedNewlySupportedBookings = 0;
        var expectedNewlyUnsupportedBookings = 3; // the three blue1 bookings are now orphaned

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, replacement);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedNewlySupportedBookings);
        proposalMetrics.NewlyUnsupportedBookingsCount.Should().Be(expectedNewlyUnsupportedBookings);
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
        
        //these tests depend on the DB state of bookings being correct with current session state
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3)
        };
        
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1), //Action removes this session
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1),
        };
        
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var expectedNewlySupportedBookings = 0;
        var expectedNewlyUnsupportedBookings = 1; // Blue1 no longer supported, Orange2 still can be supported

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, null);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedNewlySupportedBookings);
        proposalMetrics.NewlyUnsupportedBookingsCount.Should().Be(expectedNewlyUnsupportedBookings);
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
            TestBooking("3", "Blue", new DateTime(2025, 1, 1, 9, 0, 0), avStatus: "Orphaned", creationOrder: 3),
            
            TestBooking("4", "Blue", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "Orange", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Supported", creationOrder: 5),
            TestBooking("6", "Blue", new DateTime(2025, 1, 2, 9, 0, 0), avStatus: "Orphaned", creationOrder: 6),
            
            TestBooking("7", "Blue", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 7),
            TestBooking("8", "Orange", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Supported", creationOrder: 8),
            TestBooking("9", "Blue", new DateTime(2025, 1, 3, 9, 0, 0), avStatus: "Orphaned", creationOrder: 9)
        };
        
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0), ["Green", "Blue"],
                capacity: 1), //action removes session
            TestSession(new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
            
            TestSession(new DateTime(2025, 1, 2, 9, 0, 0), new DateTime(2025, 1, 2, 10, 0, 0), ["Green", "Blue"],
                capacity: 1), //action removes session
            TestSession(new DateTime(2025, 1, 2, 9, 0, 0), new DateTime(2025, 1, 2, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
            
            TestSession(new DateTime(2025, 1, 3, 9, 0, 0), new DateTime(2025, 1, 3, 10, 0, 0), ["Green", "Blue"],
                capacity: 1), //action removes session
            TestSession(new DateTime(2025, 1, 3, 9, 0, 0), new DateTime(2025, 1, 3, 10, 0, 0), ["Green", "Orange"],
                capacity: 1),
        };
        
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var expectedNewlySupportedBookings = 0;
        var expectedNewlyUnsupportedBookings = 3;

        var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, null);

        proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedNewlySupportedBookings);
        proposalMetrics.NewlyUnsupportedBookingsCount.Should().Be(expectedNewlyUnsupportedBookings);
    }

    [Fact(Skip = "Wildcard implementation needs re-developing when required.")]
    public Task AvailabilityChangeProposal_CancelAllSessions_SingleDay()
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
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3)
        };
        var sessions = new List<LinkedSessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1), //action cancels session
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1), //action cancels session
        };
        
        SetupAvailabilityAndBookings(bookings, sessions);
        return Task.CompletedTask;

        // var expectedNewlySupportedBookings = 0;
        //
        // //should this be 2??
        // var expectedNewlyUnsupportedBookings = 3;
    
        // var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, matcher, null, true);
        //
        // proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        // proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedNewlySupportedBookings);
        // proposalMetrics.NewlyUnsupportedBookingsCount.Should().Be(expectedNewlyUnsupportedBookings);
    }

    [Fact(Skip = "Wildcard implementation needs re-developing when required.")]
    public Task AvailabilityChangeProposal_CancelAllSessions_MultipleDays()
    {
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
        return Task.CompletedTask;

        //what should this metric be???
        // var expectedNewlySupportedBookings = 0;
        // var expectedNewlyUnsupportedBookings = 6;

        // var proposalMetrics = await Sut.GenerateSessionProposalActionMetrics(MockSite, from, to, null, null, true);
        // proposalMetrics.Should().BeOfType(typeof(AvailabilityUpdateProposal));
        // proposalMetrics.NewlySupportedBookingsCount.Should().Be(expectedNewlySupportedBookings);
        // proposalMetrics.NewlyUnsupportedBookingsCount.Should().Be(expectedNewlyUnsupportedBookings);
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
