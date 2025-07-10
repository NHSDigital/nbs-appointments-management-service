namespace Nhs.Appointments.Core.Reports.SiteSummary;

public interface IDailySiteSummaryStore
{
    Task CreateDailySiteSummary(DailySiteSummary summary);

    Task<IEnumerable<DailySiteSummary>> GetSiteSummarys(string site, DateOnly from, DateOnly to);
}
