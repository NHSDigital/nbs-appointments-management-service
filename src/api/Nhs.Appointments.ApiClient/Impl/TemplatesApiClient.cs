using Microsoft.Extensions.Logging;
using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.ApiClient.Impl
{
    public class TemplatesApiClient : ApiClientBase, ITemplatesApiClient
    {
        public TemplatesApiClient(Func<HttpClient> httpClientFactory, ILogger<TemplatesApiClient> logger) : base(httpClientFactory, logger)
        {
        }

        public Task<GetTemplateResponse> GetTemplate(string site) => Get<GetTemplateResponse>($"api/templates?site={site}");

        public Task<GetTemplateAssignmentsResponse> GetTemplateAssignments(string site) => Get<GetTemplateAssignmentsResponse>($"api/templates/assignments?site={site}");

        public Task<string> SetTemplate(WeekTemplate weekTemplate) => Post<WeekTemplate, string>("api/template", weekTemplate);

        public Task SetTemplateAssignment(string site, TemplateAssignment[] templates) => Post("api/templates/assignments", new SetTemplateAssignmentRequest { Assignments = templates, Site = site });

    }
}
