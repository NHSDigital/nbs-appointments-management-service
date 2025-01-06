using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/SiteSearch.feature")]
public sealed class SiteSearchFeatureSteps : SiteManagementBaseFeatureSteps, IDisposable
{
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private IEnumerable<SiteWithDistance> _actualResponse;

    [When("I make the following request with access needs")]
    public async Task RequestSitesWithAccessNeeds(Gherkin.Ast.DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;
        var accessNeeds = row.Cells.ElementAt(4).Value;
        _response = await Http.GetAsync($"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&accessNeeds={accessNeeds}&ignoreCache=true");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(await _response.Content.ReadAsStreamAsync());
    }

    [When("I make the following request without access needs")]
    public async Task RequestSitesWithoutAccessNeeds(Gherkin.Ast.DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;
        _response = await Http.GetAsync($"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&ignoreCache=true");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(await _response.Content.ReadAsStreamAsync());
    }

    [Then("the following sites and distances are returned")]
    public void Assert(Gherkin.Ast.DataTable dataTable)
    {
        var expectedSites = dataTable.Rows.Skip(1).Select(row => new SiteWithDistance(
            new Site(
                Id: GetSiteId(row.Cells.ElementAt(0).Value),
                Name: row.Cells.ElementAt(1).Value,
                Address: row.Cells.ElementAt(2).Value,
                PhoneNumber: row.Cells.ElementAt(3).Value,
                Region: row.Cells.ElementAt(4).Value,
                IntegratedCareBoard: row.Cells.ElementAt(5).Value,
                AttributeValues: ParseAttributes(row.Cells.ElementAt(6).Value),
                Location: new Location(Type: "Point", Coordinates: new[] { double.Parse(row.Cells.ElementAt(7).Value), double.Parse(row.Cells.ElementAt(8).Value) })
                ), Distance: int.Parse(row.Cells.ElementAt(9).Value)
            )).ToList();

        _statusCode.Should().Be(HttpStatusCode.OK);
        _actualResponse.Should().BeEquivalentTo(expectedSites);
    }

    public void Dispose()
    {
        var testId = GetTestId;
        DeleteSiteData(Client, testId).GetAwaiter().GetResult();
    }

    private static async Task DeleteSiteData(CosmosClient cosmosClient, string testId)
    {
        const string partitionKey = "site";
        var container = cosmosClient.GetContainer("appts", "core_data");
        using var feed = container.GetItemLinqQueryable<SiteDocument>().Where(sd => sd.Id.Contains(testId)).ToFeedIterator();
        while (feed.HasMoreResults)
        {
            var documentsResponse = await feed.ReadNextAsync();
            foreach (var document in documentsResponse)
            {
                await container.DeleteItemStreamAsync(document.Id, new PartitionKey(partitionKey));
            }
        }
    }
}
