using FluentAssertions;
using FluentAssertions.Common;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;
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

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;
public abstract class QueryAvailabilityByDaysFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled), IDisposable
{
    private HttpResponseMessage Response { get; set; }
    private HttpStatusCode StatusCode { get; set; }
    private List<AvailabilityByDays> AvailabilityResponse;
    
    private string _siteId;

    public void Dispose()
    {
        var testId = GetTestId;
        DeleteSiteData(Client, testId).GetAwaiter().GetResult();
    }
    
    [When("I query availability by days")]
    public async Task Query(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;
        
        var sites = cells.ElementAt(0).Value.Split(',');
        var services = cells.ElementAt(1).Value.Split(',');

        if (sites.Length == 1)
        {
            _siteId = sites[0];
        }

        var siteCollection = sites.Select(GetSiteId).ToArray();
        var attendeesCollection = services.Select(service => new Attendee
        {
            Services = [service]
        }).ToList();
        
        var payload = new AvailabilityQueryRequest(
            siteCollection,
            attendeesCollection,
            NaturalLanguageDate.Parse(cells.ElementAt(2).Value),
            NaturalLanguageDate.Parse(cells.ElementAt(3).Value));

        await SendRequestAsync(payload);
    }

    [When("I pass an invalid payload")]
    public async Task PassInvalidPayload()
    {
        var payload = new AvailabilityQueryRequest([], [], new DateOnly(2025, 9, 1), new DateOnly(2025, 10, 1));

        await SendRequestAsync(payload);
    }

    [Then("the following single site availability by days is returned")]
    public void AssertAvailabilityByDays(DataTable dataTable)
    {
        var availability = new AvailabilityByDays
        {
            Site = GetSiteId(_siteId),
            Days = [.. dataTable.Rows.Skip(1).Select(r =>
            {
                var cells = r.Cells;
                var dateString = cells.ElementAt(0).Value;
                var spec = cells.ElementAt(1).Value;
                var from = cells.ElementAt(2).Value;
                var until = cells.ElementAt(3).Value;

                return ParseDayEntry(dateString, spec, from, until);
            })]
        };

        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        AvailabilityResponse.Should().BeEquivalentTo([availability]);
    }

    [Then("the following multi-site availability is returned")]
    public void AssertAvailabilityMultiSite(DataTable dataTable)
    {
        var rows = dataTable.Rows.Skip(1);

        var availability = rows.Select(r =>
        {
            var cells = r.Cells;
            var site = cells.ElementAt(0).Value;
            var dateString = cells.ElementAt(1).Value;
            var spec = cells.ElementAt(2).Value;
            var from = cells.ElementAt(3).Value;
            var until = cells.ElementAt(4).Value;
            var expectedDayEntries = ExpectDayEntries(spec, from, until);

            return new AvailabilityByDays
            {
                Site = GetSiteId(site),
                Days = expectedDayEntries ? [ParseDayEntry(dateString, spec, from, until)] : []
            };
        });

        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        AvailabilityResponse.Should().BeEquivalentTo(availability);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Then("the response should be empty")]
    public void AssertEmptyResponse()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        AvailabilityResponse.SelectMany(x => x.Days).Should().BeNullOrEmpty();
    }

    private async Task SendRequestAsync(object payload)
    {
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await Http.PostAsync("http://localhost:7071/api/availability/query/days", content);
        StatusCode = Response.StatusCode;
        (_, AvailabilityResponse) =
            await JsonRequestReader.ReadRequestAsync<List<AvailabilityByDays>>(
                await Response.Content.ReadAsStreamAsync());
    }

    private static DayEntry ParseDayEntry(string dateString, string spec, string from, string until)
    {
        var date = NaturalLanguageDate.Parse(dateString);

        var blocks = ParseBlocks(spec, from, until);

        return new DayEntry
        {
            Date = date,
            Blocks = blocks
        };
    }

    private static List<Block> ParseBlocks(string spec, string from, string until)
    {
        var blocks = new List<Block>();

        foreach (var b in ParseBlockSpec(spec))
        {
            blocks.Add(b == TimeOfDayBlock.AM
                ? new Block { From = from, Until = "12:00" }
                : new Block { From = "12:00", Until = until });
        }

        return blocks;
    }

    private static TimeOfDayBlock[] ParseBlockSpec(string spec) =>
        [.. spec.Split(',').Select(s => Enum.Parse<TimeOfDayBlock>(s.Trim(), ignoreCase: true))];

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

    private static bool ExpectDayEntries(string spec, string from, string until)
    {
        return !string.IsNullOrEmpty(spec) && !string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(until);
    }

    private enum TimeOfDayBlock
    {
        AM,
        PM
    }

    [Collection(FeatureToggleCollectionNames.MultiServiceJointBookingsCollection)]
    [FeatureFile("./Scenarios/Availability/QueryAvailabilityByDays_Enabled.feature")]
    public class QueryAvailabilityByDays_Enabled() : QueryAvailabilityByDaysFeatureSteps(Flags.MultiServiceJointBookings, true);

    [Collection(FeatureToggleCollectionNames.MultiServiceJointBookingsCollection)]
    [FeatureFile("./Scenarios/Availability/QueryAvailabilityByDays_Disabled.feature")]
    public class QueryAvailabilityByDays_Disabled() : QueryAvailabilityByDaysFeatureSteps(Flags.MultiServiceJointBookings, false);
}
