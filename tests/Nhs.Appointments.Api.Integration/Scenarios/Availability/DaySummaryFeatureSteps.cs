using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
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
            var requestUrl = $"http://localhost:7071/api/day-summary?site={siteId}&from={ParseNaturalLanguageDateOnly(from).ToString("yyyy-MM-dd")}";

            _response = await Http.GetAsync(requestUrl);
            _statusCode = _response.StatusCode;

            var (_, result) =
                await JsonRequestReader.ReadRequestAsync<Summary>(await _response.Content.ReadAsStreamAsync());
            _actualResponse = result;
        }

        [And("the following session summaries on day '(.+)' are returned")]
        [Then("the following session summaries on day '(.+)' are returned")]
        public void AssertSessionSummaries(string date, DataTable expectedSessionSummaryTable)
        {
            var expectedSessionSummaries = expectedSessionSummaryTable.Rows.Skip(1).Select(row =>
                new SessionSummary
                {
                    UkStartDatetime = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToDateTime(TimeOnly.Parse(row.Cells.ElementAt(1).Value)),
                    UkEndDatetime = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToDateTime(TimeOnly.Parse(row.Cells.ElementAt(2).Value)),
                    Bookings = GetServiceBookings(row.Cells.ElementAt(3).Value),
                    Capacity = int.Parse(row.Cells.ElementAt(4).Value),
                    SlotLength = int.Parse(row.Cells.ElementAt(5).Value),
                    MaximumCapacity = int.Parse(row.Cells.ElementAt(6).Value)
                });

            var expectedDate = ParseNaturalLanguageDateOnly(date);

            _statusCode.Should().Be(HttpStatusCode.OK);
            _actualResponse.DaySummaries.Single(x => x.Date == expectedDate).Sessions.Should().BeEquivalentTo(expectedSessionSummaries,
                //exclude ID as randomly generated for linking purposes
                options => options.Excluding(x => x.Id));
        }

        [Then("the following day summary metrics are returned")]
        [And("the following day summary metrics are returned")]
        public void AssertDaySummaryMetrics(DataTable expectedDaySummaryTable)
        {
            var expectedDaySummaries = expectedDaySummaryTable.Rows.Skip(1).Select(row =>
                new DaySummary(ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value), [])
                {
                    MaximumCapacity = int.Parse(row.Cells.ElementAt(1).Value),
                    RemainingCapacity = int.Parse(row.Cells.ElementAt(2).Value),
                    BookedAppointments = int.Parse(row.Cells.ElementAt(3).Value),
                    OrphanedAppointments = int.Parse(row.Cells.ElementAt(4).Value),
                    CancelledAppointments = int.Parse(row.Cells.ElementAt(5).Value),
                });

            _statusCode.Should().Be(HttpStatusCode.OK);
            _actualResponse.DaySummaries.Should().BeEquivalentTo(expectedDaySummaries,
                //exclude sessions as should be asserted elsewhere
                options => options.Excluding(x => x.Sessions));
        }

        private static Dictionary<string, int> GetServiceBookings(string cellValue)
        {
            var serviceBookings = cellValue.Split(',');
            return serviceBookings.Select(serviceBooking => serviceBooking.Trim().Split(':')).ToDictionary(parts => parts[0], parts => int.Parse(parts[1]));
        }
    }

    [FeatureFile("./Scenarios/Availability/DaySummary_MultipleServices.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class DaySummaryFeatureSteps_MultipleServicesEnabled()
        : DaySummaryFeatureSteps(Flags.MultipleServices, true);

    [FeatureFile("./Scenarios/Availability/DaySummary_SingleService.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class DaySummaryFeatureSteps_MultipleServicesDisabled()
        : DaySummaryFeatureSteps(Flags.MultipleServices, false);
}
