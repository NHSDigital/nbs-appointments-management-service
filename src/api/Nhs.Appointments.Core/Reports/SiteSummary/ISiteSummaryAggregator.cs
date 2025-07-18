namespace Nhs.Appointments.Core.Reports.SiteSummary;

public interface ISiteSummaryAggregator
{
    Task AggregateForSite(string site, DateOnly from, DateOnly to);
}
