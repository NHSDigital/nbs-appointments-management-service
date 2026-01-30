using FluentAssertions;
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
using Okta.Sdk.Model;
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
using static Nhs.Appointments.Core.Availability.AvailabilityByHours;
using static Nhs.Appointments.Core.Availability.AvailabilityBySlots;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;
public abstract class QueryAvailabilityBySlotsFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private HttpResponseMessage Response { get; set; }
    private HttpStatusCode StatusCode { get; set; }
    private AvailabilityBySlots AvailabilityResponse { get; set; }

    private string _siteId;
    private List<Attendee> _attendeesCollection;

    [When("I query availability by slots")]
    public async Task Query(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;

        _siteId = cells.ElementAt(0).Value;
        var services = cells.ElementAt(1).Value.Split(',');
        _attendeesCollection = [.. services.Select(service => new Attendee
        {
            Services = [service]
        })];

        var date = NaturalLanguageDate.Parse(cells.ElementAt(2).Value);
        var from = TimeOnly.Parse(cells.ElementAt(3).Value);
        var until = TimeOnly.Parse(cells.ElementAt(4).Value);

        var payload = new AvailabilityQueryBySlotsRequest(
            GetSiteId(_siteId),
            _attendeesCollection,
            date.ToDateTime(from),
            date.ToDateTime(until));

        await SendRequestAsync(payload);
    }

    [When("I pass an invalid payload")]
    public async Task PassInvalidPayload()
    {
        var payload = new AvailabilityQueryBySlotsRequest(
            string.Empty,
            [],
            default,
            default);

        await SendRequestAsync(payload);
    }

    [Then("the following availability is returned for '(.+)' between '(.+)' and '(.+)'")]
    public void AssertAvailability(string date, string from, string until, DataTable dataTable)
    {
        var expectedDate = NaturalLanguageDate.Parse(date);
        var expectedSlots = dataTable.Rows.Skip(1).Select(row =>
            new Slot
            {
                From = row.Cells.ElementAt(0).Value,
                Until = row.Cells.ElementAt(1).Value,
                Services = row.Cells.ElementAt(2).Value.Split(',')
            });

        var fromTime = TimeOnly.Parse(from);
        var untilTime = TimeOnly.Parse(until);

        var expectedAvailability = new AvailabilityBySlots
        {
            Attendees = _attendeesCollection,
            From = expectedDate.ToDateTime(fromTime),
            Until = expectedDate.ToDateTime(untilTime),
            Slots = [.. expectedSlots],
            Site = GetSiteId(_siteId)
        };

        StatusCode.Should().Be(HttpStatusCode.OK);
        AvailabilityResponse.Should().BeEquivalentTo(expectedAvailability);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => StatusCode.Should().Be((HttpStatusCode)statusCode);

    private async Task SendRequestAsync(object payload)
    {
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await GetHttpClientForTest().PostAsync("http://localhost:7071/api/availability/query/slots", content);
        StatusCode = Response.StatusCode;
        (_, AvailabilityResponse) =
            await JsonRequestReader.ReadRequestAsync<AvailabilityBySlots>(
                await Response.Content.ReadAsStreamAsync());
    }

    [Collection(FeatureToggleCollectionNames.MultiServiceJointBookingsCollection)]
    [FeatureFile("./Scenarios/Availability/QueryAvailabilityBySlots_Enabled.feature")]
    public class QueryAvailabilityBySlots_Enabled() : QueryAvailabilityBySlotsFeatureSteps(Flags.MultiServiceJointBookings, true);

    [Collection(FeatureToggleCollectionNames.MultiServiceJointBookingsCollection)]
    [FeatureFile("./Scenarios/Availability/QueryAvailabilityBySlots_Disabled.feature")]
    public class QueryAvailabilityBySlots_Disabled() : QueryAvailabilityBySlotsFeatureSteps(Flags.MultiServiceJointBookings, false);
}
