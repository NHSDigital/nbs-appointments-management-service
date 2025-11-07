using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CancelSession;

[FeatureFile("./Scenarios/CancelSession/CancelSession.feature")]
public abstract class CancelSessionFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private Core.Booking _actualResponse;
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;

    [When("I cancel the following session")]
    public async Task CancelSession(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;
        var date = DateTime.ParseExact(NaturalLanguageDate.Parse(cells.ElementAt(0).Value).ToString("yyyy-MM-dd"),
            "yyyy-MM-dd", null);

        var services = cells.ElementAt(3).Value;

        object payload = new
        {
            site = GetSiteId(),
            from = DateOnly.FromDateTime(date),
            to = DateOnly.FromDateTime(date),
            sessionMatcher = new
            {
                from = cells.ElementAt(1).Value,
                until = cells.ElementAt(2).Value,
                services = services.Split(',').Select(s => s.Trim()).ToArray(),
                slotLength = int.Parse(cells.ElementAt(4).Value),
                capacity = int.Parse(cells.ElementAt(5).Value)
            },
            sessionReplacement = null as Session
        };

        _response = await Http.PostAsJsonAsync("http://localhost:7071/api/session/edit", payload);
    }

    [When("I query for a booking with the reference number '(.+)'")]
    public async Task QueryBooking(string reference)
    {
        var customId = CreateCustomBookingReference(reference);
        _response = await Http.GetAsync($"http://localhost:7071/api/booking/{customId}");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) =
            await JsonRequestReader.ReadRequestAsync<Core.Booking>(await _response.Content.ReadAsStreamAsync());
    }

    [Then("the following booking is returned")]
    public void AssertBooking(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        var expectedBooking = new Core.Booking
        {
            Reference = cells.ElementAt(4).Value,
            From = DateTime.ParseExact(
                $"{NaturalLanguageDate.Parse(cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {cells.ElementAt(1).Value}",
                "yyyy-MM-dd HH:mm", null),
            Duration = int.Parse(cells.ElementAt(2).Value),
            Service = cells.ElementAt(3).Value,
            Site = GetSiteId(),
        };

        var customId = CreateCustomBookingReference(expectedBooking.Reference);

        _actualResponse.Reference.Should().Be(customId);
        _actualResponse.From.Should().Be(expectedBooking.From);
        _actualResponse.Duration.Should().Be(expectedBooking.Duration);
        _actualResponse.Service.Should().Be(expectedBooking.Service);
        _actualResponse.Site.Should().Be(expectedBooking.Site);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => _response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Collection("CancelSessionUpliftedJourneyToggle")]
    [FeatureFile("./Scenarios/CancelSession/CancelSession.feature")]
    public class CancelSessionFeatureSteps_Enabled() : CancelSessionFeatureSteps(Flags.ChangeSessionUpliftedJourney, true);

    [Collection("CancelSessionUpliftedJourneyToggle")]
    [FeatureFile("./Scenarios/CancelSession/CancelSession_Disabled.feature")]
    public class CancelSessionFeatureSteps_Disabled() : CancelSessionFeatureSteps(Flags.ChangeSessionUpliftedJourney, false);
}

