using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/SiteSearch.feature")]
public sealed class SiteSearchFeatureSteps : SiteManagementBaseFeatureSteps, IDisposable
{
    private IEnumerable<SiteWithDistance> _sitesResponse;
    private QueryAvailabilityResponse _queryResponse;
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;

    public void Dispose()
    {
        var testId = GetTestId;
        DeleteSiteData(Client, testId).GetAwaiter().GetResult();
    }

    [When("I make the following request with access needs")]
    public async Task RequestSitesWithAccessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;
        var accessNeeds = row.Cells.ElementAt(4).Value;
        _response = await Http.GetAsync(
            $"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&accessNeeds={accessNeeds}&ignoreCache=true");
        _statusCode = _response.StatusCode;
        (_, _sitesResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await _response.Content.ReadAsStreamAsync());
    }
    
    [When(@"I check ([\w:]+) availability for site '(.+)' for '(.+)' between '(.+)' and '(.+)'")]
    public async Task CheckAvailability(string queryType, string site, string service, string from, string until)
    {
        var convertedQueryType = queryType switch
        {
            "daily" => QueryType.Days,
            "hourly" => QueryType.Hours,
            "slot" => QueryType.Slots,
            _ => throw new Exception($"{queryType} is not a valid queryType")
        };

        var payload = new
        {
            sites = new[] { GetSiteId(site) },
            service,
            from = ParseNaturalLanguageDateOnly(from),
            until = ParseNaturalLanguageDateOnly(until),
            queryType = convertedQueryType.ToString()
        };

        _response = await Http.PostAsJsonAsync($"http://localhost:7071/api/availability/query", payload);
        _statusCode = _response.StatusCode;
        (_, _queryResponse) = await JsonRequestReader.ReadRequestAsync<QueryAvailabilityResponse>(await _response.Content.ReadAsStreamAsync());
    }

    [Then(@"the following availability is returned for '(.+)'")]
    [And(@"the following availability is returned for '(.+)'")]
    public void Assert(string date, Gherkin.Ast.DataTable expectedHourlyAvailabilityTable)
    {
        var expectedDate = ParseNaturalLanguageDateOnly(date);
        var expectedHourBlocks = expectedHourlyAvailabilityTable.Rows.Skip(1).Select(row =>
            new QueryAvailabilityResponseBlock(
                TimeOnly.ParseExact(row.Cells.ElementAt(0).Value, "HH:mm"),
                TimeOnly.ParseExact(row.Cells.ElementAt(1).Value, "HH:mm"),
                int.Parse(row.Cells.ElementAt(2).Value)
            ));

        var expectedAvailability = new QueryAvailabilityResponseInfo(
            expectedDate,
            expectedHourBlocks);

        _statusCode.Should().Be(HttpStatusCode.OK);
        _queryResponse
            .Single().availability
            .Single(x => x.date == expectedDate)
            .Should().BeEquivalentTo(expectedAvailability, options => options.WithStrictOrdering());
    }
    
    [When("I make the following request with service filtering and with access needs")]
    public async Task RequestSitesWithServiceFilteringAndAccessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;

        var service = row.Cells.ElementAt(4).Value;
        var from = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(5).Value);
        var until = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(6).Value);

        var accessNeeds = row.Cells.ElementAt(7).Value;

        _response = await Http.GetAsync(
            $"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&services={service}&from={from.ToString("yyyy-MM-dd")}&until={until.ToString("yyyy-MM-dd")}&accessNeeds={accessNeeds}&ignoreCache=true");
        _statusCode = _response.StatusCode;
        (_, _sitesResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await _response.Content.ReadAsStreamAsync());
    }

    [When("I make the following request with service filtering")]
    public async Task RequestSitesWithServiceFiltering(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;

        var service = row.Cells.ElementAt(4).Value;

        var from = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(5).Value);
        var until = ParseNaturalLanguageDateOnly(row.Cells.ElementAt(6).Value);

        _response = await Http.GetAsync(
            $"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&services={service}&from={from.ToString("yyyy-MM-dd")}&until={until.ToString("yyyy-MM-dd")}&ignoreCache=true");
        _statusCode = _response.StatusCode;
        (_, _sitesResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await _response.Content.ReadAsStreamAsync());
    }

    [When("I make the following request without access needs")]
    public async Task RequestSitesWithoutAccessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;

        _response = await Http.GetAsync(
            $"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&ignoreCache=true");
        _statusCode = _response.StatusCode;
        (_, _sitesResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await _response.Content.ReadAsStreamAsync());
    }

    [Then("the following sites and distances are returned")]
    public void Assert(DataTable dataTable)
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
                    })
            ), Distance: int.Parse(row.Cells.ElementAt(11).Value)
        )).ToList();

        _sitesResponse.Should().HaveCount(dataTable.Rows.Count() - 1);

        _statusCode.Should().Be(HttpStatusCode.OK);
        _sitesResponse.Should().BeEquivalentTo(expectedSites);
    }

    [Then("no sites are returned")]
    public void Assert()
    {
        _statusCode.Should().Be(HttpStatusCode.OK);
        _sitesResponse.Should().BeEmpty();
    }

    private static async Task DeleteSiteData(CosmosClient cosmosClient, string testId)
    {
        const string partitionKey = "site";
        var container = cosmosClient.GetContainer("appts", "core_data");
        using var feed = container.GetItemLinqQueryable<SiteDocument>().Where(sd => sd.Id.Contains(testId))
            .ToFeedIterator();
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
