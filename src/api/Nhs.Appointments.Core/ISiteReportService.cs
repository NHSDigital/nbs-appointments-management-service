namespace Nhs.Appointments.Core;

public interface ISiteReportService
{
    Task<IEnumerable<SiteReport>> Generate(Site[] sites, DateOnly startDate, DateOnly endDate);
}
