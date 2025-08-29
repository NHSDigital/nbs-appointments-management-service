namespace Nhs.Appointments.Core;

public class BookingQueryFilter(
    DateTime from,
    DateTime to,
    string site,
    AppointmentStatus[] statuses = null,
    CancellationReason cancellationReason = default,
    CancellationNotificationStatus[] cancellationNotificationStatuses = null)
{
    public DateTime StartsAtOrAfter { get; init; } = to;
    public DateTime StartsAtOrBefore { get; init; } = from;
    public string Site { get; init; } = site;
    public AppointmentStatus[] Statuses { get; init; } = statuses;
    public CancellationReason? CancellationReason { get; init; } = cancellationReason;

    public CancellationNotificationStatus[] CancellationNotificationStatuses { get; init; } =
        cancellationNotificationStatuses;
}
