using System.Threading.Tasks;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability
{
    public abstract class DaySummaryFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
    {
        private Summary _actualResponse;
        private HttpResponseMessage _response;
        private HttpStatusCode _statusCode;

        [Then(@"the call should fail with (\d*)")]
        public void AssertFailureCode(int statusCode) => _statusCode.Should().Be((HttpStatusCode)statusCode);

        [When(@"I query day summary for the current site on '(.+)'")]
        public async Task QueryDaySummary(string from)
        {
            var siteId = GetSiteId();
            var requestUrl =
                $"http://localhost:7071/api/day-summary?site={siteId}&from={ParseNaturalLanguageDateOnly(from).ToString("yyyy-MM-dd")}";

            _response = await Http.GetAsync(requestUrl);
            _statusCode = _response.StatusCode;

            var (_, result) =
                await JsonRequestReader.ReadRequestAsync<AvailabilitySummary>(
                    await _response.Content.ReadAsStreamAsync());
            _actualResponse = result;
        }
    }
}
