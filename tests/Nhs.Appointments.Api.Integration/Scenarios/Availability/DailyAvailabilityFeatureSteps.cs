using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability
{
    [FeatureFile("./Scenarios/Availability/DailyAvailability.feature")]
    public abstract class DailyAvailabilityFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
    {
        private HttpResponseMessage _response;
        private HttpStatusCode _statusCode;
        private List<DailyAvailability> _actualResponse;

        [When(@"I check daily availability for the current site between '(.+)' and '(.+)'")]
        public async Task CheckDailyAvailability(string from, string until)
        {
            var siteId = GetSiteId();
            var fromDate = ParseNaturalLanguageDateOnly(from).ToString("yyyy-MM-dd");
            var untilDate = ParseNaturalLanguageDateOnly(until).ToString("yyyy-MM-dd");
            var requestUrl = $"http://localhost:7071/api/daily-availability?site={siteId}&from={fromDate}&until={untilDate}";

            _response = await Http.GetAsync(requestUrl);
            _statusCode = _response.StatusCode;

            (_, var result) = await JsonRequestReader.ReadRequestAsync<IEnumerable<DailyAvailability>>(await _response.Content.ReadAsStreamAsync());
            _actualResponse = result.ToList();
        }

        [Then("the following daily availability is returned")]
        public void AssertDailyAvailability(DataTable expectedDailyAvailabilityTable)
        {
            var expectedDailyAvailability = expectedDailyAvailabilityTable.Rows.Skip(1).Select(row =>
                new DailyAvailability
                {
                    Date = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value),
                    Sessions =
                    [
                        new() {
                            From = TimeOnly.Parse(row.Cells.ElementAt(1).Value),
                            Until = TimeOnly.Parse(row.Cells.ElementAt(2).Value),
                            Services = [row.Cells.ElementAt(3).Value],
                            SlotLength = int.Parse(row.Cells.ElementAt(4).Value),
                            Capacity = int.Parse(row.Cells.ElementAt(5).Value)
                        }
                    ]
                });

            _statusCode.Should().Be(HttpStatusCode.OK);
            _actualResponse.Count.Should().Be(2);
            _actualResponse.Should().BeEquivalentTo(expectedDailyAvailability);
        }
    }

    [Collection("MultipleServicesSerialToggle")]
    public class DailyAvailabilityFeatureSteps_MultipleServicesEnabled()
        : DailyAvailabilityFeatureSteps(Flags.MultipleServices, true);
    
    [Collection("MultipleServicesSerialToggle")]
    public class DailyAvailabilityFeatureSteps_MultipleServicesDisabled()
        : DailyAvailabilityFeatureSteps(Flags.MultipleServices, false);
}
