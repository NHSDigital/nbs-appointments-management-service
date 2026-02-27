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

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;
public abstract class QueryAvailabilityByHoursFeatureSteps(string flag, bool enabled) : SingleFeatureToggledSteps(flag, enabled)
{
    private HttpResponseMessage Response { get; set; }
    private HttpStatusCode StatusCode { get; set; }
    private AvailabilityByHours AvailabilityResponse { get; set; }

    private List<Attendee> _attendeesCollection;

    [When("I query availability by hours at the default site")]
    public async Task Query(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;

        var services = cells.ElementAt(0).Value.Split(',');
        _attendeesCollection = services.Select(service => new Attendee
        {
            Services = [service]
        }).ToList();

        var payload = new AvailabilityQueryByHoursRequest
        (
            GetSiteId(),
            _attendeesCollection,
            NaturalLanguageDate.Parse(cells.ElementAt(1).Value)
        );

        await SendRequestAsync(payload);
    }

    [When("I pass an invalid payload")]
    public async Task PassInvalidPayload()
    {
        var payload = new AvailabilityQueryByHoursRequest(
            string.Empty,
            [],
            new DateOnly(2025, 9, 1));

        await SendRequestAsync(payload);
    }

    [Then("the following '(.+)' availability is returned for '(.+)' at the default site")]
    public void AssertAvailability(string services, string date, DataTable dataTable)
    {
        var expectedServices = services.Split(',');
        var expectedDate = NaturalLanguageDate.Parse(date);
        var expectedHours = dataTable.Rows.Skip(1).Select(row =>
            new Hour
            {
                From = row.Cells.ElementAt(0).Value,
                Until = row.Cells.ElementAt(1).Value
            });

        var expectedAvailability = new AvailabilityByHours
        {
            Attendees = _attendeesCollection,
            Date = expectedDate,
            Hours = [.. expectedHours],
            Site = GetSiteId()
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
        Response = await GetHttpClientForTest().PostAsync("http://localhost:7071/api/availability/query/hours", content);
        StatusCode = Response.StatusCode;
        (_, AvailabilityResponse) =
            await JsonRequestReader.ReadRequestAsync<AvailabilityByHours>(
                await Response.Content.ReadAsStreamAsync());
    }

    [Collection(FeatureToggleCollectionNames.MultiServiceJointBookingsCollection)]
    [FeatureFile("./Scenarios/Availability/QueryAvailabilityByHours_Enabled.feature")]
    public class QueryAvailabilityByHours_Enabled() : QueryAvailabilityByHoursFeatureSteps(Flags.MultiServiceJointBookings, true);

    [Collection(FeatureToggleCollectionNames.MultiServiceJointBookingsCollection)]
    [FeatureFile("./Scenarios/Availability/QueryAvailabilityByHours_Disabled.feature")]
    public class QueryAvailabilityByHours_Disabled() : QueryAvailabilityByHoursFeatureSteps(Flags.MultiServiceJointBookings, false);
}
