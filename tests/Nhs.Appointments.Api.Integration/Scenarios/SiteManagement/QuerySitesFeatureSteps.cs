using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Persistance.Models;
using Xunit;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Sites.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

/// <summary>
/// Tests that depend on the setup and async disposal of sites due to location (long lat) search collisions
/// </summary>
public abstract class SiteLocationDependentFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled), IAsyncLifetime
{
    private HttpResponseMessage Response { get; set; }
    private HttpStatusCode StatusCode { get; set; }
    private IEnumerable<SiteWithDistance> _sitesWithDistanceResponse;
    private QueryAvailabilityResponse _queryResponse;
    
    private ErrorMessageResponseItem ErrorResponse { get; set; }

    private IEnumerable<ErrorMessageResponseItem> ErrorResponses { get; set; }

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        var testId = GetTestId;
        await DeleteSiteData(Client, testId);
    }
    
    [Given("The site '(.+)' does not exist in the system")]
    public Task NoSite()
    {
        return Task.CompletedTask;
    }

    [Then("a message is returned saying the site is not found")]
    public async Task AssertNotFound()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (_, ErrorResponse) =
            await JsonRequestReader.ReadRequestAsync<ErrorMessageResponseItem>(
                await Response.Content.ReadAsStreamAsync());
        ErrorResponse.Message.Should().Be("The specified site was not found.");
    }

    [Then("a bad request response is returned with the following error messages")]
    public async Task AssertBadRequest(DataTable dataTable)
    {
        Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var row = dataTable.Rows.ElementAt(0);

        var expectedErrorMessages = row.Cells.Select(x=>x.Value).ToList();

        (_, ErrorResponses) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<ErrorMessageResponseItem>>(
                await Response.Content.ReadAsStreamAsync());
        var actualErrorMessages = ErrorResponses.Select(x => x.Message).ToList();
        actualErrorMessages.Should().BeEquivalentTo(expectedErrorMessages);
    }

    [Then("the correct information for site '(.+)' is returned")]
    public async Task AssertSiteInformation(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var siteId = row.Cells.ElementAt(0).Value;
        var expectedSite = new Site(
            Id: GetSiteId(siteId),
            Name: row.Cells.ElementAt(1).Value,
            Address: row.Cells.ElementAt(2).Value,
            PhoneNumber: row.Cells.ElementAt(3).Value,
            OdsCode: row.Cells.ElementAt(4).Value,
            Region: row.Cells.ElementAt(5).Value,
            IntegratedCareBoard: row.Cells.ElementAt(6).Value,
            InformationForCitizens: row.Cells.ElementAt(7).Value,
            Accessibilities: ParseAccessibilities(row.Cells.ElementAt(8).Value),
            new Location(
                Type: "Point",
                Coordinates: [double.Parse(row.Cells.ElementAt(9).Value), double.Parse(row.Cells.ElementAt(10).Value)]),
            status: null,
            isDeleted: dataTable.GetBoolRowValueOrDefault(row, "IsDeleted"),
            Type: dataTable.GetRowValueOrDefault(row, "Type")
        );
        Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var actualResult = await Client.GetContainer("appts", "core_data")
            .ReadItemAsync<Site>(GetSiteId(siteId), new PartitionKey("site")); 
        actualResult.Resource.Should().BeEquivalentTo(expectedSite, opts => opts.WithStrictOrdering());
    }
    
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

        await PostQuerySitesRequestAsync(payload);
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

        await PostQuerySitesRequestAsync(payload);
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
                new SiteFilter
                {
                    Longitude = double.Parse(cells.ElementAt(0).Value),
                    Latitude = double.Parse(cells.ElementAt(1).Value),
                    SearchRadius = int.Parse(cells.ElementAt(2).Value),
                    Availability = new AvailabilityFilter
                    {
                        Services = cells.ElementAt(3).Value.Split(','),
                        From = NaturalLanguageDate.Parse(cells.ElementAt(4).Value),
                        Until = NaturalLanguageDate.Parse(cells.ElementAt(5).Value)
                    }
                }
            }
        };

        await PostQuerySitesRequestAsync(payload);
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
                    Availability = new AvailabilityFilter
                    {
                        Services = []
                    },
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
                    Availability = new AvailabilityFilter
                    {
                        Services = []
                    }
                }
            }
        };

        await PostQuerySitesRequestAsync(payload);
    }

    [When("I query sites by location")]
    public async Task QueryByLocation(DataTable dataTable)
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
                    Availability = new AvailabilityFilter
                    {
                        Services = []
                    },
                    Types = [],
                    OdsCode = string.Empty,
                    AccessNeeds = []
                },
            }
        };

        await PostQuerySitesRequestAsync(payload);
    }

    [When("I query sites with a low maxRecord count")]
    public async Task QueryMaxRecords(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;
        var payload = new
        {
            maxRecords = int.Parse(cells.ElementAt(3).Value),
            ignoreCache = true,
            filters = new[]
            {
                new SiteFilter
                {
                    Longitude = double.Parse(cells.ElementAt(0).Value),
                    Latitude = double.Parse(cells.ElementAt(1).Value),
                    SearchRadius = int.Parse(cells.ElementAt(2).Value),
                    Availability = new AvailabilityFilter
                    {
                        Services = []
                    },
                    Types = [],
                    OdsCode = string.Empty,
                    AccessNeeds = []
                },
            }
        };

        await PostQuerySitesRequestAsync(payload);
    }

    [When("I query sites by multiple filters in reverse priority order")]
    public async Task QueryMultipleFiltersReversePriority(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;
        var payload = new
        {
            maxRecords = int.Parse(cells.ElementAt(5).Value),
            ignoreCache = true,
            filters = new[]
            {
                new SiteFilter
                {
                    Longitude = double.Parse(cells.ElementAt(0).Value),
                    Latitude = double.Parse(cells.ElementAt(1).Value),
                    SearchRadius = int.Parse(cells.ElementAt(2).Value),
                    AccessNeeds = cells.ElementAt(3).Value.Split(','),
                    Availability = new AvailabilityFilter
                    {
                        Services = []
                    },
                    Types = [],
                    OdsCode = string.Empty,
                    Priority = 2
                },
                new SiteFilter
                {
                    Longitude = double.Parse(cells.ElementAt(0).Value),
                    Latitude = double.Parse(cells.ElementAt(1).Value),
                    SearchRadius = int.Parse(cells.ElementAt(2).Value),
                    Types = cells.ElementAt(4).Value.Split(','),
                    OdsCode = string.Empty,
                    AccessNeeds = [],
                    Availability = new AvailabilityFilter
                    {
                        Services = []
                    },
                    Priority = 1
                }
            }
        };

        await PostQuerySitesRequestAsync(payload);
    }

    [When("I query sites by multiple services, site type and access needs")]
    public async Task QueryServices_SiteType_AcessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;
        var payload = new
        {
            maxRecords = int.Parse(cells.ElementAt(5).Value),
            ignoreCache = true,
            filters = new[]
            {
                // Service filter
                new SiteFilter
                {
                    Longitude = double.Parse(cells.ElementAt(0).Value),
                    Latitude = double.Parse(cells.ElementAt(1).Value),
                    SearchRadius = int.Parse(cells.ElementAt(2).Value),
                    AccessNeeds = [],
                    Availability = new AvailabilityFilter
                    {
                        Services = cells.ElementAt(6).Value.Split(','),
                        From = NaturalLanguageDate.Parse(cells.ElementAt(7).Value),
                        Until = NaturalLanguageDate.Parse(cells.ElementAt(8).Value)
                    },
                    Types = [],
                    OdsCode = string.Empty
                },
                // Access needs filter
                new SiteFilter
                {
                    Longitude = double.Parse(cells.ElementAt(0).Value),
                    Latitude = double.Parse(cells.ElementAt(1).Value),
                    SearchRadius = int.Parse(cells.ElementAt(2).Value),
                    AccessNeeds = cells.ElementAt(3).Value.Split(','),
                    Availability = new AvailabilityFilter
                    {
                        Services = []
                    },
                    Types = [],
                    OdsCode = string.Empty
                },
                // Site type filter
                new SiteFilter
                {
                    Longitude = double.Parse(cells.ElementAt(0).Value),
                    Latitude = double.Parse(cells.ElementAt(1).Value),
                    SearchRadius = int.Parse(cells.ElementAt(2).Value),
                    Types = cells.ElementAt(4).Value.Split(','),
                    OdsCode = string.Empty,
                    AccessNeeds = [],
                    Availability = new AvailabilityFilter
                    {
                        Services = []
                    }
                }
            }
        };

        await PostQuerySitesRequestAsync(payload);
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
                new Location("Point",
                    Coordinates:
                    [
                        double.Parse(row.Cells.ElementAt(9).Value), double.Parse(row.Cells.ElementAt(10).Value)
                    ]),
                status: null,
                isDeleted: dataTable.GetBoolRowValueOrDefault(row, "IsDeleted"),
                Type: dataTable.GetRowValueOrDefault(row, "Type")
            ), Distance: int.Parse(row.Cells.ElementAt(11).Value)
        )).ToList();

        _sitesWithDistanceResponse.Should().HaveCount(dataTable.Rows.Count() - 1);

        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        _sitesWithDistanceResponse.Should().BeEquivalentTo(expectedSites);
        _sitesWithDistanceResponse.Select(s => s.Distance).Should().BeInAscendingOrder();
    }

    [Then("no sites are returned")]
    public void AssertNoSitesReturned()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        _sitesWithDistanceResponse.Should().BeEmpty();
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => StatusCode.Should().Be((HttpStatusCode)statusCode);
    
    [When("I make the 'get sites by area' request with access needs")]
    public async Task RequestSitesByAreaWithAccessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;
        var accessNeeds = row.Cells.ElementAt(4).Value;
        Response = await Http.GetAsync(
            $"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&accessNeeds={accessNeeds}&ignoreCache=true");
        (_, _sitesWithDistanceResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await Response.Content.ReadAsStreamAsync());
    }
    
    [When("I make the 'query sites' request with access needs")]
    public async Task QuerySitesWithAccessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;
        var accessNeeds = row.Cells.ElementAt(4).Value;
        
        var payload = new
        {
            maxRecords,
            filters = new[]
            {
                new SiteFilter
                {
                    Longitude = double.Parse(longitude),
                    Latitude = double.Parse(latitude),
                    SearchRadius = int.Parse(searchRadiusNumber),
                    AccessNeeds = accessNeeds.Split(',')
                }
            }
        };
        
        await PostQuerySitesRequestAsync(payload);
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
            from = NaturalLanguageDate.Parse(from),
            until = NaturalLanguageDate.Parse(until),
            queryType = convertedQueryType.ToString()
        };

        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/availability/query", payload);
        (_, _queryResponse) = await JsonRequestReader.ReadRequestAsync<QueryAvailabilityResponse>(await Response.Content.ReadAsStreamAsync());
    }

    [Then(@"the following availability is returned for '(.+)'")]
    [And(@"the following availability is returned for '(.+)'")]
    public void Assert(string date, DataTable expectedHourlyAvailabilityTable)
    {
        var expectedDate = NaturalLanguageDate.Parse(date);
        var expectedHourBlocks = expectedHourlyAvailabilityTable.Rows.Skip(1).Select(row =>
            new QueryAvailabilityResponseBlock(
                TimeOnly.ParseExact(row.Cells.ElementAt(0).Value, "HH:mm"),
                TimeOnly.ParseExact(row.Cells.ElementAt(1).Value, "HH:mm"),
                int.Parse(row.Cells.ElementAt(2).Value)
            ));

        var expectedAvailability = new QueryAvailabilityResponseInfo(
            expectedDate,
            expectedHourBlocks);

        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        _queryResponse
            .Single().availability
            .Single(x => x.date == expectedDate)
            .Should().BeEquivalentTo(expectedAvailability, options => options.WithStrictOrdering());
    }
    
    [When("I make the 'get sites by area' request with service filtering and with access needs")]
    public async Task RequestSitesByAreaWithServiceFilteringAndAccessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;

        var services = row.Cells.ElementAt(4).Value;
        var from = NaturalLanguageDate.Parse(row.Cells.ElementAt(5).Value);
        var until = NaturalLanguageDate.Parse(row.Cells.ElementAt(6).Value);

        var accessNeeds = row.Cells.ElementAt(7).Value;

        Response = await Http.GetAsync(
            $"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&services={services}&from={from:yyyy-MM-dd}&until={until:yyyy-MM-dd}&accessNeeds={accessNeeds}&ignoreCache=true");
        (_, _sitesWithDistanceResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await Response.Content.ReadAsStreamAsync());
    }
    
    [When("I make the 'query sites' request with service filtering and with access needs")]
    public async Task QuerySitesWithServiceFilteringAndAccessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;

        var services = row.Cells.ElementAt(4).Value;
        var from = NaturalLanguageDate.Parse(row.Cells.ElementAt(5).Value);
        var until = NaturalLanguageDate.Parse(row.Cells.ElementAt(6).Value);

        var accessNeeds = row.Cells.ElementAt(7).Value;
        
        var payload = new
        {
            maxRecords,
            filters = new[]
            {
                new SiteFilter
                {
                    Longitude = double.Parse(longitude),
                    Latitude = double.Parse(latitude),
                    SearchRadius = int.Parse(searchRadiusNumber),
                    AccessNeeds = accessNeeds.Split(','),
                    Availability = new AvailabilityFilter()
                    {
                        From = from,
                        Until = until,
                        Services = services.Split(',')
                    }
                }
            }
        };
        
        await PostQuerySitesRequestAsync(payload);
    }
    
    [When("I make the 'get sites by area' request with service filtering, access needs, and caching")]
    public async Task RequestSitesByAreaWithServiceFilteringAndAccessNeedsAndCacheEnabled(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;

        var services = row.Cells.ElementAt(4).Value;
        var from = NaturalLanguageDate.Parse(row.Cells.ElementAt(5).Value);
        var until = NaturalLanguageDate.Parse(row.Cells.ElementAt(6).Value);

        var accessNeeds = row.Cells.ElementAt(7).Value;

        Response = await Http.GetAsync(
            $"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&services={services}&from={from:yyyy-MM-dd}&until={until:yyyy-MM-dd}&accessNeeds={accessNeeds}&ignoreCache=false");
        (_, _sitesWithDistanceResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await Response.Content.ReadAsStreamAsync());
    }
    
    [When("I make the 'query sites' request with service filtering, access needs, and caching")]
    public async Task QuerySitesWithServiceFilteringAndAccessNeedsAndCacheEnabled(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;

        var services = row.Cells.ElementAt(4).Value;
        var from = NaturalLanguageDate.Parse(row.Cells.ElementAt(5).Value);
        var until = NaturalLanguageDate.Parse(row.Cells.ElementAt(6).Value);

        var accessNeeds = row.Cells.ElementAt(7).Value;
        
        var payload = new
        {
            maxRecords,
            ignoreCache = false,
            filters = new[]
            {
                new SiteFilter
                {
                    Longitude = double.Parse(longitude),
                    Latitude = double.Parse(latitude),
                    SearchRadius = int.Parse(searchRadiusNumber),
                    AccessNeeds = accessNeeds.Split(','),
                    Availability = new AvailabilityFilter()
                    {
                        From = from,
                        Until = until,
                        Services = services.Split(',')
                    }
                }
            }
        };

        await PostQuerySitesRequestAsync(payload);
    }
    
    //Not to be used unless explicitly need to wait
    [When("I wait for '(.+)' milliseconds")]
    public async Task WaitForSeconds(string milliseconds)
    {
        var timespan = TimeSpan.FromMilliseconds(int.Parse(milliseconds));
        await Task.Delay(timespan);
    }

    [When("I make the 'get sites by area' request with service filtering")]
    public async Task RequestSitesByAreaWithServiceFiltering(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;

        var services = row.Cells.ElementAt(4).Value;

        var from = NaturalLanguageDate.Parse(row.Cells.ElementAt(5).Value);
        var until = NaturalLanguageDate.Parse(row.Cells.ElementAt(6).Value);

        Response = await Http.GetAsync(
            $"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&services={services}&from={from:yyyy-MM-dd}&until={until:yyyy-MM-dd}&ignoreCache=true");
        (_, _sitesWithDistanceResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await Response.Content.ReadAsStreamAsync());
    }
    
    [When("I make the 'query sites' request with service filtering")]
    public async Task QuerySitesWithServiceFiltering(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;

        var services = row.Cells.ElementAt(4).Value;

        var from = NaturalLanguageDate.Parse(row.Cells.ElementAt(5).Value);
        var until = NaturalLanguageDate.Parse(row.Cells.ElementAt(6).Value);
        
        var payload = new
        {
            maxRecords,
            filters = new[]
            {
                new SiteFilter
                {
                    Longitude = double.Parse(longitude),
                    Latitude = double.Parse(latitude),
                    SearchRadius = int.Parse(searchRadiusNumber),
                    Availability = new AvailabilityFilter
                    {
                        From = from,
                        Until = until,
                        Services = services.Split(',')
                    }
                }
            }
        };

        await PostQuerySitesRequestAsync(payload);
    }

    [When("I make the 'get sites by area' request without access needs")]
    public async Task RequestSitesByAreaWithoutAccessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;
        Response = await Http.GetAsync(
            $"http://localhost:7071/api/sites?long={longitude}&lat={latitude}&searchRadius={searchRadiusNumber}&maxRecords={maxRecords}&ignoreCache=true");
        (_, _sitesWithDistanceResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await Response.Content.ReadAsStreamAsync());
    }
    
    [When("I make the 'query sites' request without access needs")]
    public async Task QuerySitesWithoutAccessNeeds(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var maxRecords = row.Cells.ElementAt(0).Value;
        var searchRadiusNumber = row.Cells.ElementAt(1).Value;
        var longitude = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;
        
        var payload = new
        {
            maxRecords,
            filters = new[]
            {
                new SiteFilter
                {
                    Longitude = double.Parse(longitude),
                    Latitude = double.Parse(latitude),
                    SearchRadius = int.Parse(searchRadiusNumber)
                }
            }
        };

        await PostQuerySitesRequestAsync(payload);
    }

    [Then("no sites are returned")]
    public void Assert()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        _sitesWithDistanceResponse.Should().BeEmpty();
    }
    
    [When("I update the details for site '(.+)'")]
    public async Task UpdateSiteDetails(string siteDesignation, DataTable dataTable)
    {
        var siteId = GetSiteId(siteDesignation);
        var row = dataTable.Rows.ElementAt(1);
        
        var name = row.Cells.ElementAt(0).Value;
        var address = row.Cells.ElementAt(1).Value;
        var phoneNumber = row.Cells.ElementAt(2).Value;
        var longitude = row.Cells.ElementAt(3).Value;
        var latitude = row.Cells.ElementAt(4).Value;

        var payload = new SetSiteDetailsRequest(siteId, name, address, phoneNumber, longitude, latitude);
        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/sites/{siteId}/details", payload);
    }

    private async Task PostQuerySitesRequestAsync(object payload)
    {
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await Http.PostAsync("http://localhost:7071/api/sites", content);
        StatusCode = Response.StatusCode;
        (_, _sitesWithDistanceResponse) =
            await JsonRequestReader.ReadRequestAsync<IEnumerable<SiteWithDistance>>(
                await Response.Content.ReadAsStreamAsync());
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

    [Collection(FeatureToggleCollectionNames.QuerySitesCollection)]
    [FeatureFile("./Scenarios/SiteManagement/SiteLocationDependent_QuerySitesEnabled.feature")]
    public class SiteLocationDependent_QuerySitesEnabled() : SiteLocationDependentFeatureSteps(Flags.QuerySites, true);
    
    [Collection(FeatureToggleCollectionNames.QuerySitesCollection)]
    [FeatureFile("./Scenarios/SiteManagement/SiteLocationDependent_QuerySitesDisabled.feature")]
    public class SiteLocationDependent_QuerySitesDisabled() : SiteLocationDependentFeatureSteps(Flags.QuerySites, false);
}
