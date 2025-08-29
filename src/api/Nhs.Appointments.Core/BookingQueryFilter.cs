namespace Nhs.Appointments.Core;

public class BookingQueryFilter(
    DateTime from,
    DateTime to,
    string site,
    AppointmentStatus[] statuses,
    CancellationReason cancellationReason,
    CancellationNotificationStatus[] cancellationNotificationStatuses)
{
    public DateTime StartsAtOrAfter { get; init; } = to;
    public DateTime StartsAtOrBefore { get; init; } = from;
    public string Site { get; init; } = site;
    public AppointmentStatus[] Statuses { get; init; } = statuses;
    public CancellationReason? CancellationReason { get; init; } = cancellationReason;

    public CancellationNotificationStatus[] CancellationNotificationStatuses { get; init; } =
        cancellationNotificationStatuses;
}
