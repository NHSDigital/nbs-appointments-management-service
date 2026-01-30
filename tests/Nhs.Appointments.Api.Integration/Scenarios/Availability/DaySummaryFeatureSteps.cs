using System.Threading.Tasks;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core.Availability;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability
{
    [FeatureFile("./Scenarios/Availability/DaySummary.feature")]
    public class DaySummaryFeatureSteps : AvailabilitySummaryFeatureSteps
    {
        [When(@"I query day summary for the current site on '(.+)'")]
        public async Task QueryDaySummary(string from)
        {
            var siteId = GetSiteId();
            var requestUrl =
                $"http://localhost:7071/api/day-summary?site={siteId}&from={NaturalLanguageDate.Parse(from).ToString("yyyy-MM-dd")}";

            _response = await GetHttpClientForTest().GetAsync(requestUrl);
            _statusCode = _response.StatusCode;

            var (_, result) =
                await JsonRequestReader.ReadRequestAsync<AvailabilitySummary>(
                    await _response.Content.ReadAsStreamAsync());
            _actualResponse = result;
        }
    }
}
