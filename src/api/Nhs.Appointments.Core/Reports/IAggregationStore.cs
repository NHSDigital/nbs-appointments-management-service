namespace Nhs.Appointments.Core.Reports;

public interface IAggregationStore
{
    Task<DateTimeOffset?> GetLastRunDate(string id);
    Task SetLastRunDate(string id, DateTimeOffset lastTriggerUtcDate);
}
