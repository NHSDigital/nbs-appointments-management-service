using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/SiteSearch.feature")]
public sealed class SiteSearchFeatureSteps : SiteManagementBaseFeatureSteps
{
    private HttpResponseMessage? _response;
    private HttpStatusCode _statusCode;
    private IEnumerable<SiteWithDistance>? _actualResponse;
    
    [When("I make the following request")]
    public async Task RequestSites(Gherkin.Ast.DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;
        _response = await Http.GetAsync($"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}");
        _statusCode = _response.StatusCode;
        _actualResponse = await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(await _response.Content.ReadAsStreamAsync());
    }

    [Then("the following sites and distances are returned")]
    public void Assert(Gherkin.Ast.DataTable dataTable)
    {
        var expectedSites = dataTable.Rows.Skip(1).Select(row => new SiteWithDistance(
            new Site(
                Id: row.Cells.ElementAt(0).Value, 
                Name: row.Cells.ElementAt(1).Value,
                Address: row.Cells.ElementAt(2).Value,
                AttributeValues: new AttributeValue[]{ new(row.Cells.ElementAt(3).Value, row.Cells.ElementAt(4).Value)},
                Location: new Location(Type: "Point", Coordinates: new[] { double.Parse(row.Cells.ElementAt(5).Value), double.Parse(row.Cells.ElementAt(6).Value) })
                ), Distance: int.Parse(row.Cells.ElementAt(7).Value)
            )).ToList();

        _statusCode.Should().Be(HttpStatusCode.OK);
        _actualResponse.Should().BeEquivalentTo(expectedSites);
    }
}

