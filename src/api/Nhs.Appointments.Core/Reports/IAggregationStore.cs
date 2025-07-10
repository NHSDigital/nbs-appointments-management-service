namespace Nhs.Appointments.Core.Reports;

public interface IAggregationStore
{
    Task<DateTime?> GetLastRunDate(string id);
    Task GetLastRunDate(string id, DateTime lastRunDateUtc);
}
