using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/GetSiteBySiteId.feature")]
public sealed class GetSiteByIdFeatureSteps : SiteManagementBaseFeatureSteps
{
    private HttpResponseMessage? _response;
    private HttpStatusCode _statusCode;
    private Site? _actualResponse;

    [When("I request site details for site '(.+)'")]
    public async Task RequestSites(string siteId)
    {
        _response = await Http.GetAsync($"http://localhost:7071/api/sites/{siteId}");
        _statusCode = _response.StatusCode;
        _actualResponse = await JsonRequestReader.ReadRequestAsync<Site>(await _response.Content.ReadAsStreamAsync());
    }
    
    [Then("the correct site is returned")]
    public void Assert(Gherkin.Ast.DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var expectedSite = new Site(
            Id: row.Cells.ElementAt(0).Value,
            Name: row.Cells.ElementAt(1).Value,
            Address: row.Cells.ElementAt(2).Value,
            AttributeValues: new[]{new AttributeValue(row.Cells.ElementAt(3).Value, row.Cells.ElementAt(4).Value)},
            Location: new Location(
                Type: "Point",
                Coordinates: [double.Parse(row.Cells.ElementAt(5).Value), double.Parse(row.Cells.ElementAt(6).Value)])
            );
        _statusCode.Should().Be(HttpStatusCode.OK);
        _actualResponse.Should().BeEquivalentTo(expectedSite);
    }
}
