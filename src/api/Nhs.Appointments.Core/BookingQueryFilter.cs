namespace Nhs.Appointments.Core;

public class BookingQueryFilter(
    DateTime from,
    DateTime to,
    string site,
    AppointmentStatus[] statuses = null,
    CancellationReason? cancellationReason = null,
    CancellationNotificationStatus[] cancellationNotificationStatuses = null)
{
    public DateTime StartsAtOrAfter { get; init; } = from;
    public DateTime StartsAtOrBefore { get; init; } = to;
    public string Site { get; init; } = site;
    public AppointmentStatus[] Statuses { get; init; } = statuses;
    public CancellationReason? CancellationReason { get; init; } = cancellationReason;

    public CancellationNotificationStatus[] CancellationNotificationStatuses { get; init; } =
        cancellationNotificationStatuses;

    public bool Matches(Booking booking)
    {
        if (booking == null)
        {
            return false;
        }

        var siteMatch = booking.Site == Site;
        var dateRangeMatch = booking.From >= StartsAtOrAfter &&
                             booking.From <= StartsAtOrBefore;

        var statusMatch = Statuses?.Contains(booking.Status) ?? true;
        var cancellationReasonMatch = CancellationReason == null ||
                                      booking.CancellationReason == CancellationReason;
        var notificationStatusMatch = CancellationNotificationStatuses is null ||
                                      CancellationNotificationStatuses.Any(status =>
                                          booking.CancellationNotificationStatus == status);

        return siteMatch && dateRangeMatch && statusMatch && cancellationReasonMatch && notificationStatusMatch;
    }
}
