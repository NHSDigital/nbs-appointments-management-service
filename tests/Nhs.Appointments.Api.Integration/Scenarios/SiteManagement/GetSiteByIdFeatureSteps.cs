using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/GetSiteBySiteId.feature")]
public sealed class GetSiteByIdFeatureSteps : SiteManagementBaseFeatureSteps
{
    [When("I request site details for site '(.+)' with the scope '(.+)'")]
    public async Task RequestSites(string siteDesignation, string scope)
    {
        var siteId = GetSiteId(siteDesignation);
        Response = await Http.GetAsync($"http://localhost:7071/api/sites/{siteId}?scope={scope}");
    }

    [Then("the correct site is returned")]
    public async Task Assert(Gherkin.Ast.DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var expectedSite = new Site(
            id: GetSiteId(row.Cells.ElementAt(0).Value),
            name: row.Cells.ElementAt(1).Value,
            address: row.Cells.ElementAt(2).Value,
            phoneNumber: row.Cells.ElementAt(3).Value,
            region: row.Cells.ElementAt(4).Value,
            integratedCareBoard: row.Cells.ElementAt(5).Value,
            attributeValues: ParseAttributes(row.Cells.ElementAt(6).Value),
            location: new Location(
                Type: "Point",
                Coordinates: [double.Parse(row.Cells.ElementAt(7).Value), double.Parse(row.Cells.ElementAt(8).Value)])
        );
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        (_, ActualResponse) = await JsonRequestReader.ReadRequestAsync<Site>(await Response.Content.ReadAsStreamAsync());
        ActualResponse.Should().BeEquivalentTo(expectedSite);
    }
}
