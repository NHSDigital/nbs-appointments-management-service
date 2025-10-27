using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;
public abstract class QuerySitesFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private HttpResponseMessage Response { get; set; }
    private HttpStatusCode StatusCode { get; set; }
    private IEnumerable<SiteWithDistance> _sitesResponse;

    [When("I query sites by site type and ODS code")]
    [And("I query sites by site type and ODS code")]
    public async Task QueryBySiteTypeAndOdsCode(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;
        var payload = new
        {
            maxRecords = 50,
            ignoreCache = true,
            filters = new []
            {
                new
                {
                    types = cells.ElementAt(0).Value.Split(','),
                    odsCode = cells.ElementAt(1).Value,
                    longitude = double.Parse(cells.ElementAt(2).Value),
                    latitude = double.Parse(cells.ElementAt(3).Value),
                    searchRadius = int.Parse(cells.ElementAt(4).Value)
                }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await Http.PostAsync("http://localhost:7071/api/sites", content);
        StatusCode = Response.StatusCode;
        (_, _sitesResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await Response.Content.ReadAsStreamAsync());
    }

    [When("I query sites by access needs")]
    public async Task QueryByAccessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;
        var payload = new
        {
            maxRecords = 50,
            ignoreCache = true,
            filters = new[]
            {
                new
                {
                    longitude = double.Parse(cells.ElementAt(0).Value),
                    latitude = double.Parse(cells.ElementAt(1).Value),
                    searchRadius = int.Parse(cells.ElementAt(2).Value),
                    accessNeeds = cells.ElementAt(3).Value.Split(',')
                }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await Http.PostAsync("http://localhost:7071/api/sites", content);
        StatusCode = Response.StatusCode;
        (_, _sitesResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await Response.Content.ReadAsStreamAsync());
    }

    [When("I query sites by service")]
    public async Task QuerySitesByService(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;
        var payload = new
        {
            maxRecords = 50,
            ignoreCache = true,
            filters = new[]
            {
                new
                {
                    longitude = double.Parse(cells.ElementAt(0).Value),
                    latitude = double.Parse(cells.ElementAt(1).Value),
                    searchRadius = int.Parse(cells.ElementAt(2).Value),
                    services = cells.ElementAt(3).Value.Split(','),
                    from = ParseNaturalLanguageDateOnly(cells.ElementAt(4).Value),
                    until = ParseNaturalLanguageDateOnly(cells.ElementAt(5).Value)
                }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await Http.PostAsync("http://localhost:7071/api/sites", content);
        StatusCode = Response.StatusCode;
        (_, _sitesResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await Response.Content.ReadAsStreamAsync());
    }

    [When("I query sites by multiple filters")]
    public async Task QuerySitesMultipleFilters(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;
        var payload = new
        {
            maxRecords = 50,
            ignoreCache = true,
            filters = new[]
            {
                new SiteFilter
                {
                    Longitude = double.Parse(cells.ElementAt(0).Value),
                    Latitude = double.Parse(cells.ElementAt(1).Value),
                    SearchRadius = int.Parse(cells.ElementAt(2).Value),
                    AccessNeeds = cells.ElementAt(3).Value.Split(','),
                    Services = [],
                    Types = [],
                    OdsCode = string.Empty
                },
                new SiteFilter
                {
                    Longitude = double.Parse(cells.ElementAt(0).Value),
                    Latitude = double.Parse(cells.ElementAt(1).Value),
                    SearchRadius = int.Parse(cells.ElementAt(2).Value),
                    Types = cells.ElementAt(4).Value.Split(','),
                    OdsCode = cells.ElementAt(5).Value,
                    AccessNeeds = [],
                    Services = []
                }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await Http.PostAsync("http://localhost:7071/api/sites", content);
        StatusCode = Response.StatusCode;
        (_, _sitesResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await Response.Content.ReadAsStreamAsync());
    }

    [Then("the following sites and distances are returned")]
    public void AssertSites(DataTable dataTable)
    {
        var expectedSites = dataTable.Rows.Skip(1).Select(row => new SiteWithDistance(
            new Site(
                Id: GetSiteId(row.Cells.ElementAt(0).Value),
                Name: row.Cells.ElementAt(1).Value,
                Address: row.Cells.ElementAt(2).Value,
                PhoneNumber: row.Cells.ElementAt(3).Value,
                OdsCode: row.Cells.ElementAt(4).Value,
                Region: row.Cells.ElementAt(5).Value,
                IntegratedCareBoard: row.Cells.ElementAt(6).Value,
                InformationForCitizens: row.Cells.ElementAt(7).Value,
                Accessibilities: ParseAccessibilities(row.Cells.ElementAt(8).Value),
                Location: new Location(Type: "Point",
                    Coordinates: new[]
                    {
                        double.Parse(row.Cells.ElementAt(9).Value), double.Parse(row.Cells.ElementAt(10).Value)
                    }),
                status: null,
                isDeleted: null,
                Type: row.Cells.ElementAt(12)?.Value ?? string.Empty
            ), Distance: int.Parse(row.Cells.ElementAt(11).Value)
        )).ToList();

        _sitesResponse.Should().HaveCount(dataTable.Rows.Count() - 1);

        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        _sitesResponse.Should().BeEquivalentTo(expectedSites);
    }

    [Then("no sites are returned")]
    public void Assert()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        _sitesResponse.Should().BeEmpty();
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Collection("QuerySitesToggle")]
    [FeatureFile("./Scenarios/SiteManagement/QuerySites_Enabled.feature")]
    public class QuerySitesFeaturesSteps_Enabled() : QuerySitesFeatureSteps(Flags.QuerySites, true);


    [Collection("QuerySitesToggle")]
    [FeatureFile("./Scenarios/SiteManagement/QuerySites_Disabled.feature")]
    public class QuerySitesFeatureSteps_Disabled() : QuerySitesFeatureSteps(Flags.QuerySites, false);
}
