namespace Nhs.Appointments.Core;

public class BookingQueryFilter
{
    public BookingQueryFilter(DateTime from,
        DateTime to,
        string site,
        string[] statuses,
        string cancellationReason,
        string[] cancellationNotificationStatuses)
    {
        StartsAtOrBefore = from;
        StartsAtOrAfter = to;
        Site = site;
        Statuses = statuses.Select(status => Enum.Parse<AppointmentStatus>(status)).ToArray();
        CancellationReason = Enum.Parse<CancellationReason>(cancellationReason);
        CancellationNotificationStatuses = cancellationNotificationStatuses
            .Select(status => Enum.Parse<CancellationNotificationStatus>(status)).ToArray();
    }

    public DateTime StartsAtOrAfter { get; set; }
    public DateTime StartsAtOrBefore { get; set; }
    public string Site { get; set; }
    public AppointmentStatus[] Statuses { get; set; }
    public CancellationReason? CancellationReason { get; set; }
    public CancellationNotificationStatus[] CancellationNotificationStatuses { get; set; }
}
