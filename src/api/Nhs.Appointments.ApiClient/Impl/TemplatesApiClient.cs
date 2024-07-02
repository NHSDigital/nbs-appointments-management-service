using Microsoft.Extensions.Logging;
using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.ApiClient.Impl
{
    public class TemplatesApiClient : ApiClientBase, ITemplatesApiClient
    {
        public TemplatesApiClient(Func<HttpClient> httpClientFactory, ILogger logger) : base(httpClientFactory, logger)
        {
        }

        public async Task<GetTemplateResponse> GetTemplate(string site)
        {
            var response = await Get<GetTemplateResponse>($"api/templates?site={site}");
            return response;
        }

        public async Task<GetTemplateAssignmentsResponse> GetTemplateAssignments(string site)
        {
            var response = await Get<GetTemplateAssignmentsResponse>($"api/templates/assignments?site={site}");
            return response;
        }

        public async Task<string> SetTemplate(WeekTemplate weekTemplate)
        {
            var response = await Post<WeekTemplate, string>(weekTemplate, "api/template");
            return response;
        }

        public async Task SetTemplateAssignment(string site, TemplateAssignment[] templates)
        {
            var request = new SetTemplateAssignmentRequest { Assignments = templates, Site = site };
            await Post(request, "api/templates/assignments");
        }
    }
}
