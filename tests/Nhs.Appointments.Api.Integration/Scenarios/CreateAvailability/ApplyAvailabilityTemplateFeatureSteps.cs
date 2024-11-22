using System.Net;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using FluentAssertions;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/ApplyAvailabilityTemplate.feature")]
    public sealed class ApplyAvailabilityTemplateFeatureSteps : BaseCreateAvailabilityFeatureSteps
    {
        [Then("the request is successful and the following daily availability is created")]
        public async Task AssertDailyAvailability(Gherkin.Ast.DataTable expectedDailyAvailabilityTable)
        {
            _statusCode.Should().Be(HttpStatusCode.OK);
            var site = GetSiteId();
            var expectedDocuments = DailyAvailabilityDocumentsFromTable(site, expectedDailyAvailabilityTable);
            var container = Client.GetContainer("appts", "booking_data");
            var actualDocuments = await RunQueryAsync<DailyAvailabilityDocument>(container, d => d.DocumentType == "daily_availability" && d.Site == site);
            actualDocuments.Should().BeEquivalentTo(expectedDocuments);
        }
    }
}
