using FluentAssertions;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance.UnitTests;

public class BookingQueryFilterExtensionsTests
{
    private readonly string TestSiteId = "4F78B37D-C5A8-4BBD-9D66-FC78B8081651";

    private DateTime TestDateAt(string time)
    {
        var hour = time.Split(':')[0];
        var minute = time.Split(':')[1];

        return new DateTime(2020, 6, 7, int.Parse(hour), int.Parse(minute), 0);
    }

    private BookingDocument TestBooking(string reference, string time = null,
        AppointmentStatus? status = null, CancellationReason? cancellationReason = null,
        CancellationNotificationStatus? cancellationNotificationStatus = null)
    {
        return new BookingDocument
        {
            Id = reference,
            DocumentType = "booking",
            Site = TestSiteId,
            Reference = reference,
            From = TestDateAt(time ?? "10:00"),
            Duration = 10,
            Service = "RSV:Adult",
            Created = new DateTime(2020, 05, 10, 10, 47, 32),
            Status = status ?? AppointmentStatus.Booked,
            AvailabilityStatus = AvailabilityStatus.Supported,
            StatusUpdated = default,
            AttendeeDetails = null,
            ContactDetails = null,
            AdditionalData = null,
            ReminderSent = false,
            CancellationReason = cancellationReason,
            CancellationNotificationStatus = cancellationNotificationStatus
        };
    }

    [Fact]
    public void FiltersBookingsByTime()
    {
        var bookings = new List<BookingDocument>
        {
            TestBooking("01", "09:00"),
            TestBooking("02", "09:10"),
            TestBooking("03", "09:20"),
            TestBooking("04", "09:30"),
            TestBooking("05", "09:40")
        };

        var filter = new BookingQueryFilter(TestDateAt("09:10"), TestDateAt("09:30"),
            TestSiteId);

        var result = bookings.Where(booking => booking.IsMatchedBy(filter)).ToList();

        result.Should().HaveCount(3);

        result.Should().NotContain(bookings[0]);
        result.Should().Contain(bookings[1]);
        result.Should().Contain(bookings[2]);
        result.Should().Contain(bookings[3]);
        result.Should().NotContain(bookings[4]);
    }

    [Fact]
    public void FiltersBookingsByStatus()
    {
        var bookings = new List<BookingDocument>
        {
            TestBooking("01", status: AppointmentStatus.Booked),
            TestBooking("02", status: AppointmentStatus.Cancelled),
            TestBooking("03", status: AppointmentStatus.Unknown),
            TestBooking("04", status: AppointmentStatus.Provisional),
            TestBooking("05", status: AppointmentStatus.Booked)
        };

        var filter = new BookingQueryFilter(TestDateAt("09:00"), TestDateAt("17:30"),
            TestSiteId, new[] { AppointmentStatus.Cancelled, AppointmentStatus.Provisional });

        var result = bookings.Where(booking => booking.IsMatchedBy(filter)).ToList();

        result.Should().HaveCount(2);

        result.Should().NotContain(bookings[0]);
        result.Should().Contain(bookings[1]);
        result.Should().NotContain(bookings[2]);
        result.Should().Contain(bookings[3]);
        result.Should().NotContain(bookings[4]);
    }

    [Fact]
    public void FiltersBookingsByCancellationReason()
    {
        var bookings = new List<BookingDocument>
        {
            TestBooking("01", cancellationReason: CancellationReason.CancelledBySite),
            TestBooking("02", cancellationReason: CancellationReason.CancelledBySite),
            TestBooking("03", cancellationReason: CancellationReason.RescheduledByCitizen),
            TestBooking("04", cancellationReason: CancellationReason.CancelledBySite),
            TestBooking("05", cancellationReason: CancellationReason.CancelledByCitizen)
        };

        var filter = new BookingQueryFilter(TestDateAt("09:00"), TestDateAt("17:30"),
            TestSiteId, cancellationReason: CancellationReason.RescheduledByCitizen);

        var result = bookings.Where(booking => booking.IsMatchedBy(filter)).ToList();

        result.Should().HaveCount(1);

        result.Should().NotContain(bookings[0]);
        result.Should().NotContain(bookings[1]);
        result.Should().Contain(bookings[2]);
        result.Should().NotContain(bookings[3]);
        result.Should().NotContain(bookings[4]);
    }

    [Fact]
    public void FiltersBookingsByCancellationNotificationStatus()
    {
        var bookings = new List<BookingDocument>
        {
            TestBooking("01", cancellationNotificationStatus: CancellationNotificationStatus.Unknown),
            TestBooking("02", cancellationNotificationStatus: CancellationNotificationStatus.ManuallyNotified),
            TestBooking("03",
                cancellationNotificationStatus: CancellationNotificationStatus.AutomaticNotificationFailed),
            TestBooking("04", cancellationNotificationStatus: CancellationNotificationStatus.Unnotified),
            TestBooking("05", cancellationNotificationStatus: CancellationNotificationStatus.ManuallyNotified)
        };

        var filter = new BookingQueryFilter(TestDateAt("09:00"), TestDateAt("17:30"),
            TestSiteId,
            cancellationNotificationStatuses: new[]
            {
                CancellationNotificationStatus.Unnotified,
                CancellationNotificationStatus.AutomaticNotificationFailed
            });

        var result = bookings.Where(booking => booking.IsMatchedBy(filter)).ToList();

        result.Should().HaveCount(2);

        result.Should().NotContain(bookings[0]);
        result.Should().NotContain(bookings[1]);
        result.Should().Contain(bookings[2]);
        result.Should().Contain(bookings[3]);
        result.Should().NotContain(bookings[4]);
    }

    [Fact]
    public void FiltersBookingsByACombinationOfFilters()
    {
        var bookings = new List<BookingDocument>
        {
            // Cancelled booking at 09:00, requires manual notification
            TestBooking("01", "09:00", AppointmentStatus.Cancelled, CancellationReason.CancelledBySite,
                CancellationNotificationStatus.Unnotified),
            // Provisional booking at 09:00
            TestBooking("02", "09:00", AppointmentStatus.Provisional, null, CancellationNotificationStatus.Unknown),
            // Booked booking at 9:10
            TestBooking("03", "09:10", AppointmentStatus.Booked, null, CancellationNotificationStatus.Unknown),
            // Rescheduled booking at 09:20
            TestBooking("04", "09:20", AppointmentStatus.Cancelled, CancellationReason.RescheduledByCitizen),
            // Cancelled booking at 09:30, by citizen
            TestBooking("05", "09:30", AppointmentStatus.Cancelled, CancellationReason.CancelledByCitizen,
                CancellationNotificationStatus.ManuallyNotified),
            // Cancelled booking at 09:40, automatically notified
            TestBooking("06", "09:40", AppointmentStatus.Cancelled, CancellationReason.CancelledBySite,
                CancellationNotificationStatus.Unknown),
            // Cancelled booking at 09:50, automatically notified but notification failed
            TestBooking("07", "09:50", AppointmentStatus.Cancelled, CancellationReason.CancelledBySite,
                CancellationNotificationStatus.AutomaticNotificationFailed),
            // Cancelled booking at 10:00, requires manual notification
            TestBooking("08", "09:50", AppointmentStatus.Cancelled, CancellationReason.CancelledBySite,
                CancellationNotificationStatus.Unnotified),
            // Orphaned booking at 10:00
            TestBooking("09", "10:00", AppointmentStatus.Booked, null, CancellationNotificationStatus.Unknown),
            // Cancelled booking at 10:10, automatically notified but notification failed
            TestBooking("10", "10:10", AppointmentStatus.Cancelled, CancellationReason.CancelledBySite,
                CancellationNotificationStatus.AutomaticNotificationFailed)
        };

        // "Find me cancelled bookings between 09:30 and 10 where the citizens could not be notified OR their automatic notification failed"
        var filter = new BookingQueryFilter(TestDateAt("09:30"), TestDateAt("10:00"),
            TestSiteId,
            new[] { AppointmentStatus.Cancelled },
            CancellationReason.CancelledBySite,
            new[]
            {
                CancellationNotificationStatus.Unnotified,
                CancellationNotificationStatus.AutomaticNotificationFailed
            });

        var result = bookings.Where(booking => booking.IsMatchedBy(filter)).ToList();

        result.Should().HaveCount(2);

        result.Should().NotContain(bookings[0]);
        result.Should().NotContain(bookings[1]);
        result.Should().NotContain(bookings[2]);
        result.Should().NotContain(bookings[3]);
        result.Should().NotContain(bookings[4]);
        result.Should().NotContain(bookings[5]);
        result.Should().Contain(bookings[6]);
        result.Should().Contain(bookings[7]);
        result.Should().NotContain(bookings[8]);
        result.Should().NotContain(bookings[9]);
    }
}
