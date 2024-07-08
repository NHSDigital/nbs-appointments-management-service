using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.ApiClient
{
    public interface ITemplatesApiClient
    {
        Task<GetTemplateAssignmentsResponse> GetTemplateAssignments(string site);
        Task<GetTemplateResponse> GetTemplate(string site);
        Task SetTemplateAssignment(string site, TemplateAssignment[] templates);
        Task<string> SetTemplate(WeekTemplate weekTemplate);
    }
}
