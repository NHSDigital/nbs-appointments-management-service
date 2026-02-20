using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;

namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class CancelDateRangeProposalTests : BookingAvailabilityStateServiceTestBase
{
    [Fact]
    public async Task CancelDateRangeProposal_FiltersOutUnsupportedBookings()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "COVID", new DateTime(2026, 1, 1, 9, 0, 0), avStatus: "Supported"),
            TestBooking("2", "RSV", new DateTime(2026, 1, 1, 9, 0, 0), avStatus: "Supported"),
            TestBooking("3", "FLu", new DateTime(2026, 1, 1, 9, 0, 0), avStatus: "Orphaned"),

            TestBooking("4", "COVID", new DateTime(2026, 1, 2, 9, 0, 0), avStatus: "Supported"),
            TestBooking("5", "RSV", new DateTime(2026, 1, 2, 9, 0, 0), avStatus: "Supported"),
            TestBooking("6", "FLu", new DateTime(2026, 1, 2, 9, 0, 0), avStatus: "Orphaned"),

            TestBooking("7", "COVID", new DateTime(2026, 1, 3, 9, 0, 0), avStatus: "Supported"),
            TestBooking("8", "RSV", new DateTime(2026, 1, 3, 9, 0, 0), avStatus: "Supported"),
            TestBooking("9", "COVID", new DateTime(2026, 1, 3, 9, 0, 0), avStatus: "Orphaned")
        };

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), ["FLU", "COVID"]),
            TestSession(new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), ["FLU", "RSV"]),

            TestSession(new DateTime(2026, 1, 2, 9, 0, 0), new DateTime(2026, 1, 2, 10, 0, 0), ["FLU", "COVID"]),
            TestSession(new DateTime(2026, 1, 2, 9, 0, 0), new DateTime(2026, 1, 2, 10, 0, 0), ["FLU", "RSV"]),

            TestSession(new DateTime(2026, 1, 3, 9, 0, 0), new DateTime(2026, 1, 3, 10, 0, 0), ["FLU", "COVID"]),
            TestSession(new DateTime(2026, 1, 3, 9, 0, 0), new DateTime(2026, 1, 3, 10, 0, 0), ["FLU", "RSV"]),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var expectedBookingCount = 6;
        var expectedSessionCount = 6;

        var (SessionCount, BookingCount) = await Sut.GenerateCancelDateRangeProposalActionMetricsAsync("some-site", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2));

        SessionCount.Should().Be(expectedSessionCount);
        BookingCount.Should().Be(expectedBookingCount);
    }

    [Fact]
    public async Task CancelDateRangeProposal_FiltersOutLiveStatusBookings()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "COVID", new DateTime(2026, 1, 1, 9, 0, 0), avStatus: "Supported"),
            TestBooking("2", "RSV", new DateTime(2026, 1, 1, 9, 0, 0), avStatus: "Supported"),
            TestBooking("3", "FLu", new DateTime(2026, 1, 1, 9, 0, 0), avStatus: "Supported", status: "Provisional"),

            TestBooking("4", "COVID", new DateTime(2026, 1, 2, 9, 0, 0), avStatus: "Supported"),
            TestBooking("5", "RSV", new DateTime(2026, 1, 2, 9, 0, 0), avStatus: "Supported"),
            TestBooking("6", "FLu", new DateTime(2026, 1, 2, 9, 0, 0), avStatus: "Supported", status: "Provisional"),

            TestBooking("7", "COVID", new DateTime(2026, 1, 3, 9, 0, 0), avStatus: "Supported"),
            TestBooking("8", "RSV", new DateTime(2026, 1, 3, 9, 0, 0), avStatus: "Supported"),
            TestBooking("9", "COVID", new DateTime(2026, 1, 3, 9, 0, 0), avStatus: "Supported", status: "Provisional")
        };

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), ["FLU", "COVID"]),
            TestSession(new DateTime(2026, 1, 1, 9, 0, 0), new DateTime(2026, 1, 1, 10, 0, 0), ["FLU", "RSV"]),

            TestSession(new DateTime(2026, 1, 2, 9, 0, 0), new DateTime(2026, 1, 2, 10, 0, 0), ["FLU", "COVID"]),
            TestSession(new DateTime(2026, 1, 2, 9, 0, 0), new DateTime(2026, 1, 2, 10, 0, 0), ["FLU", "RSV"]),

            TestSession(new DateTime(2026, 1, 3, 9, 0, 0), new DateTime(2026, 1, 3, 10, 0, 0), ["FLU", "COVID"]),
            TestSession(new DateTime(2026, 1, 3, 9, 0, 0), new DateTime(2026, 1, 3, 10, 0, 0), ["FLU", "RSV"]),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var expectedBookingCount = 6;
        var expectedSessionCount = 6;

        var (SessionCount, BookingCount) = await Sut.GenerateCancelDateRangeProposalActionMetricsAsync("some-site", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2));

        SessionCount.Should().Be(expectedSessionCount);
        BookingCount.Should().Be(expectedBookingCount);
    }
}
