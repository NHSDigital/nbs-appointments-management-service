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
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability
{
    [FeatureFile("./Scenarios/Availability/DaySummary.feature")]
    public class DaySummaryFeatureSteps : BaseFeatureSteps
    {
        private AvailabilitySummary _actualResponse;
        private HttpResponseMessage _response;
        private HttpStatusCode _statusCode;

        [When(@"I query day summary for the current site on '(.+)'")]
        public async Task QueryDaySummary(string from)
        {
            var siteId = GetSiteId();
            var requestUrl = $"http://localhost:7071/api/day-summary?site={siteId}&from={ParseNaturalLanguageDateOnly(from).ToString("yyyy-MM-dd")}";

            _response = await Http.GetAsync(requestUrl);
            _statusCode = _response.StatusCode;

            var (_, result) =
                await JsonRequestReader.ReadRequestAsync<AvailabilitySummary>(await _response.Content.ReadAsStreamAsync());
            _actualResponse = result;
        }

        [And("the following session summaries on day '(.+)' are returned")]
        [Then("the following session summaries on day '(.+)' are returned")]
        public void AssertSessionSummaries(string date, DataTable expectedSessionSummaryTable)
        {
            var expectedSessionSummaries = expectedSessionSummaryTable.Rows.Skip(1).Select(row =>
                new SessionAvailabilitySummary
                {
                    UkStartDatetime = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToDateTime(TimeOnly.Parse(row.Cells.ElementAt(1).Value)),
                    UkEndDatetime = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToDateTime(TimeOnly.Parse(row.Cells.ElementAt(2).Value)),
                    //TODO fix
                    // Bookings = GetServiceBookings(row.Cells.ElementAt(3).Value),
                    Capacity = int.Parse(row.Cells.ElementAt(4).Value),
                    SlotLength = int.Parse(row.Cells.ElementAt(5).Value),
                    MaximumCapacity = int.Parse(row.Cells.ElementAt(6).Value)
                });

            var expectedDate = ParseNaturalLanguageDateOnly(date);

            _statusCode.Should().Be(HttpStatusCode.OK);
            _actualResponse.DaySummaries.Single(x => x.Date == expectedDate).SessionSummaries.Should().BeEquivalentTo(expectedSessionSummaries,
                //exclude ID as randomly generated for linking purposes
                options => options.Excluding(x => x.Id));
        }

        [Then("the following day summary metrics are returned")]
        [And("the following day summary metrics are returned")]
        public void AssertDaySummaryMetrics(DataTable expectedDaySummaryTable)
        {
            var expectedDaySummaries = expectedDaySummaryTable.Rows.Skip(1).Select(row =>
                new DayAvailabilitySummary(ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value), [])
                {
                    MaximumCapacity = int.Parse(row.Cells.ElementAt(1).Value),
                    RemainingCapacity = int.Parse(row.Cells.ElementAt(2).Value),
                    // TotalSupportedAppointments = 4,
                    // OrphanedAppointments = int.Parse(row.Cells.ElementAt(4).Value),
                    // CancelledAppointments = int.Parse(row.Cells.ElementAt(5).Value),
                });
            

            _statusCode.Should().Be(HttpStatusCode.OK);
            _actualResponse.DaySummaries.First().TotalSupportedAppointments.Should().Be(expectedDaySummaries.Count());
            
            // _actualResponse.DaySummaries.Should().BeEquivalentTo(expectedDaySummaries,
            //     //exclude sessions as should be asserted elsewhere
            //     options => options.Excluding(x => x.SessionSummaries));
        }

        private static Dictionary<string, int> GetServiceBookings(string cellValue)
        {
            var serviceBookings = cellValue.Split(',');
            return serviceBookings.Select(serviceBooking => serviceBooking.Trim().Split(':')).ToDictionary(parts => parts[0], parts => int.Parse(parts[1]));
        }
    }
}
