using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/UpdateSiteAttributes.feature")]
public sealed class UpdateSiteAttributesFeatureSteps : SiteManagementBaseFeatureSteps
{
    [When("I update the attributes for site '(.+)'")]
    public async Task UpdateSiteAttributes(string siteDesignation, Gherkin.Ast.DataTable dataTable)
    {
        var siteId = GetSiteId(siteDesignation);
        var row = dataTable.Rows.ElementAt(1);
        var attributeValues = ParseAttributes(row.Cells.ElementAt(0).Value);
        var payload = new SetSiteAttributesRequest(siteId, "*", attributeValues);
        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/sites/{siteId}/attributes", payload);
    }
    
    [Then("the correct information for site '(.+)' is returned")]
    public async Task Assert(Gherkin.Ast.DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var siteId = row.Cells.ElementAt(0).Value;
        var expectedSite = new Site(
            Id: GetSiteId(siteId),
            Name: row.Cells.ElementAt(1).Value,
            Address: row.Cells.ElementAt(2).Value,
            PhoneNumber: row.Cells.ElementAt(3).Value,
            Region: row.Cells.ElementAt(4).Value,
            IntegratedCareBoard: row.Cells.ElementAt(5).Value,
            AttributeValues: ParseAttributes(row.Cells.ElementAt(6).Value),
            Location: new Location(
                Type: "Point",
                Coordinates: [double.Parse(row.Cells.ElementAt(7).Value), double.Parse(row.Cells.ElementAt(8).Value)])
        );
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var actualResult = await Client.GetContainer("appts", "index_data").ReadItemAsync<Site>(GetSiteId(siteId), new Microsoft.Azure.Cosmos.PartitionKey("site"));
        actualResult.Resource.Should().BeEquivalentTo(expectedSite);
    }
}