namespace Nhs.Appointments.Core;

public interface ISiteReportService
{
    Task<IEnumerable<SiteReport>> GenerateReports(IEnumerable<Site> sites, DateOnly startDate, DateOnly endDate);
}
