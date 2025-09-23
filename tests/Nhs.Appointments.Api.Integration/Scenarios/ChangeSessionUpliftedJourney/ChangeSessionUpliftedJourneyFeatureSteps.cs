using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.ChangeSessionUpliftedJourney;

public abstract class ChangeSessionUpliftedJourneyFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private HttpResponseMessage Response {  get; set; }

    [When("I update the following session")]
    public async Task UpdateSession(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;
        var date = DateTime.ParseExact(ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value).ToString("yyyy-MM-dd"),
            "yyyy-MM-dd", null);

        var services = cells.ElementAt(3).Value;

        object payload = new
        {
            site = GetSiteId(),
            from = DateOnly.FromDateTime(date),
            to = DateOnly.FromDateTime(date),
            sessionMatcher = "*",
            sessionReplacement = null as Session
        };

        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/session/edit", payload);
    }

    [When("I cancel all sessions in between '(.+)' and '(.+)'")]
    public async Task CancelAllSessionsInDateRange(string from, string until)
    {
        var fromDate = DateTime.ParseExact(ParseNaturalLanguageDateOnly(from).ToString("yyyy-MM-dd"),
            "yyyy-MM-dd", null);
        var untilDate = DateTime.ParseExact(ParseNaturalLanguageDateOnly(until).ToString("yyyy-MM-dd"),
            "yyyy-MM-dd", null);

        object payload = new
        {
            site = GetSiteId(),
            from = DateOnly.FromDateTime(fromDate),
            to = DateOnly.FromDateTime(untilDate),
            sessionMatcher = "*",
            sessionReplacement = null as Session
        };

        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/session/edit", payload);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Collection("ChangeSessionUpliftedJourneyToggle")]
    [FeatureFile("./Scenarios/ChangeSessionUpliftedJourney/ChangeSessionUpliftedJourney_Enabled.feature")]
    public class ChangeSessionUpliftedJourneyFeatureSteps_Enabled() : ChangeSessionUpliftedJourneyFeatureSteps(Flags.ChangeSessionUpliftedJourney, true);

    [Collection("ChangeSessionUpliftedJourneyToggle")]
    [FeatureFile("./Scenarios/ChangeSessionUpliftedJourney/ChangeSessionUpliftedJourney_Disabled.feature")]
    public class ChangeSessionUpliftedJourneyFeatureSteps_Disabled() : ChangeSessionUpliftedJourneyFeatureSteps(Flags.ChangeSessionUpliftedJourney, false);
}
