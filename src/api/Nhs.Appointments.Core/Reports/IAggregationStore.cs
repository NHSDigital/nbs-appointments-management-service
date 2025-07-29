namespace Nhs.Appointments.Core.Reports;

public interface IAggregationStore
{
    Task<DateTimeOffset?> GetLastRunDate();
    Task SetLastRunDate(DateTimeOffset lastTriggerUtcDate);
}
