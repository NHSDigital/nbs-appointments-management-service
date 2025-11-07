using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability
{
    [FeatureFile("./Scenarios/Availability/WeekSummary.feature")]
    public class WeekSummaryFeatureSteps : AvailabilitySummaryFeatureSteps
    {
        [Then(@"the call should fail with (\d*)")]
        public void AssertFailureCode(int statusCode) => _statusCode.Should().Be((HttpStatusCode)statusCode);

        [When(@"I query week summary for the current site on '(.+)'")]
        public async Task QueryWeekSummary(string from)
        {
            var siteId = GetSiteId();
            var requestUrl =
                $"http://localhost:7071/api/week-summary?site={siteId}&from={NaturalLanguageDate.Parse(from).ToString("yyyy-MM-dd")}";

            _response = await Http.GetAsync(requestUrl);
            _statusCode = _response.StatusCode;

            var (_, result) =
                await JsonRequestReader.ReadRequestAsync<AvailabilitySummary>(await _response.Content.ReadAsStreamAsync());
            _actualResponse = result;
        }
    }
}
