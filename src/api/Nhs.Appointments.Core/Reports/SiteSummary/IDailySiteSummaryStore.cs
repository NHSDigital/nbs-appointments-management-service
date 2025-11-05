namespace Nhs.Appointments.Core.Reports.SiteSummary;

public interface IDailySiteSummaryStore
{
    Task CreateDailySiteSummary(DailySiteSummary summary);

    Task<IEnumerable<DailySiteSummary>> GetSiteSummaries(string site, DateOnly from, DateOnly to);
    Task IfExistsDelete(string site, DateOnly date);
}
