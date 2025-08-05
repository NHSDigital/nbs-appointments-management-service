using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/GetSiteBySiteId.feature")]
public sealed class GetSiteByIdFeatureSteps : SiteManagementBaseFeatureSteps
{
    [When("I request site details for site '(.+)'")]
    public async Task RequestSites(string siteDesignation)
    {
        var siteId = GetSiteId(siteDesignation);
        Response = await Http.GetAsync($"http://localhost:7071/api/sites/{siteId}");
    }

    [Then("the correct site is returned")]
    public async Task Assert(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var expectedSite = new Site(
            Id: GetSiteId(row.Cells.ElementAt(0).Value),
            Name: row.Cells.ElementAt(1).Value,
            Address: row.Cells.ElementAt(2).Value,
            PhoneNumber: row.Cells.ElementAt(3).Value,
            OdsCode: row.Cells.ElementAt(4).Value,
            Region: row.Cells.ElementAt(5).Value,
            IntegratedCareBoard: row.Cells.ElementAt(6).Value,
            InformationForCitizens: row.Cells.ElementAt(7).Value,
            Accessibilities: ParseAccessibilities(row.Cells.ElementAt(8).Value),
            Location: new Location(
                Type: "Point",
                Coordinates: [double.Parse(row.Cells.ElementAt(9).Value), double.Parse(row.Cells.ElementAt(10).Value)]),
            status: null
        );
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        (_, ActualResponse) =
            await JsonRequestReader.ReadRequestAsync<Site>(await Response.Content.ReadAsStreamAsync());
        ActualResponse.Should().BeEquivalentTo(expectedSite);
    }
}
