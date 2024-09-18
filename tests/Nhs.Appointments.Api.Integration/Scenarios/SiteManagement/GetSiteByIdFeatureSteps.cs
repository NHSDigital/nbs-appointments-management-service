using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/GetSiteBySiteId.feature")]
public sealed class GetSiteByIdFeatureSteps : SiteManagementBaseFeatureSteps
{
    private HttpResponseMessage? _response;
    private Site? _actualResponse;
    private ErrorMessageResponseItem? _errorResponse;
    
    [When("I request site details for site '(.+)'")]
    public async Task RequestSites(string siteDesignation)
    {
        var siteId = GetSiteId(siteDesignation);
        _response = await Http.GetAsync($"http://localhost:7071/api/sites/{siteId}");
    }
    
    [Then("the correct site is returned")]
    public async Task Assert(Gherkin.Ast.DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var expectedSite = new Site(
            Id: GetSiteId(row.Cells.ElementAt(0).Value),
            Name: row.Cells.ElementAt(1).Value,
            Address: row.Cells.ElementAt(2).Value,
            AttributeValues: ParseAttributes(row.Cells.ElementAt(3).Value),
            Location: new Location(
                Type: "Point",
                Coordinates: [double.Parse(row.Cells.ElementAt(4).Value), double.Parse(row.Cells.ElementAt(5).Value)])
        );
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _actualResponse = await JsonRequestReader.ReadRequestAsync<Site>(await _response.Content.ReadAsStreamAsync());
        _actualResponse.Should().BeEquivalentTo(expectedSite);
    }

    [Then("a message is returned saying the site is not found")]
    public async Task Assert()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _errorResponse = await JsonRequestReader.ReadRequestAsync<ErrorMessageResponseItem>(await _response.Content.ReadAsStreamAsync());
        _errorResponse.Message.Should().Be("The specified site was not found.");
    }
}
