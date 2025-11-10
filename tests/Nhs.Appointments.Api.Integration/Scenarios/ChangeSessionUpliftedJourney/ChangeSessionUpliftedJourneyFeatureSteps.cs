using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.ChangeSessionUpliftedJourney;

public abstract class ChangeSessionUpliftedJourneyFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private HttpResponseMessage Response { get; set; }
    private Session SessionToCheck { get; set; }

    [When("I replace the session with the following")]
    public async Task UpdateSession(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var date = ParseDate(GetCell(row, 0));

        var existingSession = await GetDayAvailability(date);
        SessionToCheck = BuildSession(row, 1);

        var payload = BuildPayload(
            date, date,
            matcher: new
            {
                from = existingSession.From.ToString("HH:mm"),
                until = existingSession.Until.ToString("HH:mm"),
                services = existingSession.Services,
                slotLength = existingSession.SlotLength,
                capacity = existingSession.Capacity
            },
            replacement: new
            {
                from = GetCell(row, 1),
                until = GetCell(row, 2),
                services = GetCell(row, 3).Split(",").Select(s => s.Trim()).ToArray(),
                slotLength = GetCell(row, 4),
                capacity = GetCell(row, 5)
            });

        await SendSessionEditRequest(payload);
    }

    //Cancelling multiple days not yet implemented
    
    // [When("I cancel all sessions in between '(.+)' and '(.+)'")]
    // public async Task CancelAllSessionsInDateRange(string from, string until)
    // {
    //     var fromDate = ParseDate(from);
    //     var untilDate = ParseDate(until);
    //
    //     var payload = BuildPayload(fromDate, untilDate, matcher: "*", replacement: null as Session);
    //     await SendSessionEditRequest(payload);
    // }

    [When("I cancel the following session using the new endpoint")]
    public async Task CancelSingleSession(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var date = ParseDate(GetCell(row, 0));

        SessionToCheck = BuildSession(row, 1);

        var payload = BuildPayload(
            date, date,
            matcher: new
            {
                from = SessionToCheck.From.ToString("HH:mm"),
                until = SessionToCheck.Until.ToString("HH:mm"),
                services = SessionToCheck.Services,
                slotLength = SessionToCheck.SlotLength,
                capacity = SessionToCheck.Capacity
            },
            replacement: null as Session);

        await SendSessionEditRequest(payload);
    }

    [When("I cancel the sessions matching this between '(.+)' and '(.+)'")]
    public async Task CancelMultipleSessions(string fromDate, string untilDate, DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var from = ParseDate(fromDate);
        var until = ParseDate(untilDate);

        SessionToCheck = BuildSession(row);

        var payload = BuildPayload(
            from, until,
            matcher: new
            {
                from = SessionToCheck.From.ToString("HH:mm"),
                until = SessionToCheck.Until.ToString("HH:mm"),
                services = SessionToCheck.Services,
                slotLength = SessionToCheck.SlotLength,
                capacity = SessionToCheck.Capacity
            },
            replacement: null as Session);

        await SendSessionEditRequest(payload);
    }

    [When("I replace multiple sessions between '(.+)' and '(.+)' with this session")]
    public async Task UpdateMultipleSessions(string fromDate, string untilDate, DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var from = ParseDate(fromDate);
        var until = ParseDate(untilDate);

        var existingSession = await GetDayAvailability(from);
        SessionToCheck = BuildSession(row);

        var payload = BuildPayload(
            from, until,
            matcher: new
            {
                from = existingSession.From.ToString("HH:mm"),
                until = existingSession.Until.ToString("HH:mm"),
                services = existingSession.Services,
                slotLength = existingSession.SlotLength,
                capacity = existingSession.Capacity
            },
            replacement: new
            {
                from = GetCell(row, 0),
                until = GetCell(row, 1),
                services = GetCell(row, 2).Split(",").Select(s => s.Trim()).ToArray(),
                slotLength = GetCell(row, 3),
                capacity = GetCell(row, 4)
            });

        await SendSessionEditRequest(payload);
    }

    [Then("the session '(.+)' no longer exists")]
    public async Task AssertSessionDoesNotExist(string dateString)
    {
        var date = ParseDate(dateString);
        await AssertSessionsForDay(date, shouldExist: false);
    }

    [Then("the sessions between '(.+)' and '(.+)' no longer exist")]
    public async Task AssertMultipleSessionsRemoved(string fromDate, string untilDate)
    {
        var from = ParseDate(fromDate);
        var until = ParseDate(untilDate);

        foreach (var date in Enumerable.Range(0, (until - from).Days + 1).Select(offset => from.AddDays(offset)))
        {
            await AssertSessionsForDay(date, shouldExist: false);
        }
    }

    [Then("the sessions between '(.+)' and '(.+)' should have been updated")]
    public async Task AssertUpdatedSessions(string fromDate, string untilDate)
    {
        var from = ParseDate(fromDate);
        var until = ParseDate(untilDate);

        foreach (var date in Enumerable.Range(0, (until - from).Days + 1).Select(offset => from.AddDays(offset)))
        {
            await AssertSessionsForDay(date, shouldExist: true);
        }
    }

    [Then("the session '(.+)' should have been updated")]
    public async Task AssertSessionUpdated(string dateString)
    {
        var date = ParseDate(dateString);
        await AssertSessionsForDay(date, shouldExist: true);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    private static DateTime ParseDate(string date) =>
        DateTime.ParseExact(NaturalLanguageDate.Parse(date).ToString("yyyy-MM-dd"), "yyyy-MM-dd", null);

    private static string GetCell(TableRow row, int index) => row.Cells.ElementAt(index).Value;

    private static Session BuildSession(TableRow row, int fromIndex = 0)
    {
        var services = GetCell(row, fromIndex + 2);
        return new Session
        {
            From = TimeOnly.Parse(GetCell(row, fromIndex)),
            Until = TimeOnly.Parse(GetCell(row, fromIndex + 1)),
            Services = [.. services.Split(",").Select(s => s.Trim())],
            SlotLength = int.Parse(GetCell(row, fromIndex + 3)),
            Capacity = int.Parse(GetCell(row, fromIndex + 4))
        };
    }

    private async Task<Session> GetDayAvailability(DateTime date)
    {
        var documentId = date.ToString("yyyyMMdd");
        var dayAvailabilityDocument = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<DailyAvailabilityDocument>(documentId, new PartitionKey(GetSiteId()));
        return dayAvailabilityDocument.Resource.Sessions.First();
    }

    private async Task SendSessionEditRequest(object payload) => Response = await Http.PostAsJsonAsync("http://localhost:7071/api/session/edit", payload);

    private object BuildPayload(DateTime from, DateTime until, object matcher, object replacement) =>
        new
        {
            site = GetSiteId(),
            from = DateOnly.FromDateTime(from),
            to = DateOnly.FromDateTime(until),
            sessionMatcher = matcher,
            sessionReplacement = replacement
        };

    private async Task AssertSessionsForDay(DateTime date, bool shouldExist)
    {
        var documentId = date.ToString("yyyyMMdd");
        var dayAvailabilityDocument = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<DailyAvailabilityDocument>(documentId, new PartitionKey(GetSiteId()));

        var matchingSessions = dayAvailabilityDocument.Resource.Sessions.Where(s =>
            s.From == SessionToCheck.From &&
            s.Until == SessionToCheck.Until &&
            s.SlotLength == SessionToCheck.SlotLength &&
            s.Capacity == SessionToCheck.Capacity);

        if (shouldExist)
        {
            matchingSessions.Count().Should().Be(1);
        }
        else
        {
            matchingSessions.Should().BeEmpty();
        }
    }

    [Collection(FeatureToggleCollectionNames.ChangeSessionUpliftedJourneyCollection)]
    [FeatureFile("./Scenarios/ChangeSessionUpliftedJourney/ChangeSessionUpliftedJourney_Enabled.feature")]
    public class ChangeSessionUpliftedJourneyFeatureSteps_Enabled() : ChangeSessionUpliftedJourneyFeatureSteps(Flags.ChangeSessionUpliftedJourney, true);

    [Collection(FeatureToggleCollectionNames.ChangeSessionUpliftedJourneyCollection)]
    [FeatureFile("./Scenarios/ChangeSessionUpliftedJourney/ChangeSessionUpliftedJourney_Disabled.feature")]
    public class ChangeSessionUpliftedJourneyFeatureSteps_Disabled() : ChangeSessionUpliftedJourneyFeatureSteps(Flags.ChangeSessionUpliftedJourney, false);
}
