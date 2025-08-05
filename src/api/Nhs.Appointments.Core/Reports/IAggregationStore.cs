namespace Nhs.Appointments.Core.Reports;

public interface IAggregationStore
{
    Task<Aggregation?> GetLastRun();
    Task SetLastRun(DateTimeOffset lastTriggerUtcDate, DateOnly aggregationFrom, DateOnly aggregationTo, DateOnly dateUntilAggregated);
}
