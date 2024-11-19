using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Persistance.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/SetAvailability.feature")]
    public class SetAvailabilityFeatureSteps : CreateAvailabilityFeatureSteps
    {
        private HttpResponseMessage _response;

        [When(@"I apply the following availability")]
        public async Task SetAvailability(DataTable dataTable)
        {
            var cells = dataTable.Rows.ElementAt(1).Cells;

            var relativeDate = DeriveRelativeDateOnly(cells.ElementAt(0).Value).ToString("yyyy-MM-dd");
            var payload = new
            {
                date = relativeDate,
                site = GetSiteId(),
                sessions = new[]
                {
                    new {
                        from = cells.ElementAt(1).Value,
                        until = cells.ElementAt(2).Value,
                        slotLength = cells.ElementAt(3).Value,
                        capacity = cells.ElementAt(4).Value,
                        services = cells.ElementAt(5).Value.Split(',').Select(s => s.Trim()).ToArray(),
                    }
                }
            };

            _response = await Http.PostAsJsonAsync("http://localhost:7071/api/availability", payload);
        }

        [Then(@"the request is successful and the following availability is created")]
        public async Task AssertAvailabilityAsync(DataTable expectedDataTable)
        {
            _response.StatusCode.Should().Be(HttpStatusCode.OK);
            var site = GetSiteId();
            var expectedDocuments = DailyAvailabilityDocumentsFromTable(site, expectedDataTable);
            var container = Client.GetContainer("appts", "booking_data");
            var actualDocuments = await RunQueryAsync<DailyAvailabilityDocument>(container, d => d.DocumentType == "daily_availability" && d.Site == site);
            actualDocuments.Should().BeEquivalentTo(expectedDocuments);
        }
    }
}
