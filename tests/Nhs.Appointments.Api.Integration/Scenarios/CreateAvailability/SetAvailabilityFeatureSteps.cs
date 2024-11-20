using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Persistance.Models;
using System.Net;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/SetAvailability.feature")]
    public class SetAvailabilityFeatureSteps : BaseCreateAvailabilityFeatureSteps
    {
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
