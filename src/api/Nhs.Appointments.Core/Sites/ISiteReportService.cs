namespace Nhs.Appointments.Core.Sites;

public interface ISiteReportService
{
    Task<IEnumerable<SiteReport>> GenerateReports(IEnumerable<Site> sites, DateOnly startDate, DateOnly endDate);
}
