using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
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

namespace Nhs.Appointments.Api.Integration.Scenarios.CancelDateRange;

public abstract class CancelDateRangeFeatureSteps(FlagState[] flagStates) : MultipleFeatureToggledSteps(flagStates)
{
    private HttpResponseMessage Response { get; set; }
    private HttpStatusCode StatusCode { get; set; }
    private CancelDateRangeResponse CancelResponse { get; set; }
    private List<DailyAvailability> AvailabilityResponse { get; set; }
    private List<Core.Bookings.Booking> BookingResponse { get; set; }

    [When("I cancel sessions and bookings for the default site within a date range")]
    public async Task CancelDateRange(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;

        var payload = new CancelDateRangeRequest(
            GetSiteId(),
            NaturalLanguageDate.Parse(cells.ElementAt(0).Value),
            NaturalLanguageDate.Parse(cells.ElementAt(1).Value),
            bool.Parse(cells.ElementAt(2).Value));

        await SendRequestAsync(payload);
    }

    [When("I cancel the following session at the default site")]
    public async Task CancelSession(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;
        var date = DateTime.ParseExact(NaturalLanguageDate.Parse(cells.ElementAt(0).Value).ToString("yyyy-MM-dd"), "yyyy-MM-dd", null);

        object payload = new
        {
            site = GetSiteId(),
            date = DateOnly.FromDateTime(date),
            until = cells.ElementAt(2).Value,
            from = cells.ElementAt(1).Value,
            services = new string[] { cells.ElementAt(3).Value },
            slotLength = int.Parse(cells.ElementAt(4).Value),
            capacity = int.Parse(cells.ElementAt(5).Value)
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        _ = await GetHttpClientForTest().PostAsync("http://localhost:7071/api/session/cancel", content);
    }

    [When(@"I check daily availability for the default site between '(.+)' and '(.+)'")]
    public async Task CheckDailyAvailabilityDefaultSite(string from, string until)
    {
        var siteId = GetSiteId();
        var fromDate = NaturalLanguageDate.Parse(from).ToString("yyyy-MM-dd");
        var untilDate = NaturalLanguageDate.Parse(until).ToString("yyyy-MM-dd");
        var requestUrl = $"http://localhost:7071/api/daily-availability?site={siteId}&from={fromDate}&until={untilDate}";

        var response = await GetHttpClientForTest().GetAsync(requestUrl);

        (_, var result) = await JsonRequestReader.ReadRequestAsync<IEnumerable<DailyAvailability>>(await response.Content.ReadAsStreamAsync());
        AvailabilityResponse = [.. result];
    }

    [Then("the daily availability should be empty")]
    public void AsertEmptyAvailability()
    {
        AvailabilityResponse.ForEach(a => a.Sessions.Should().BeEmpty());
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Then("the following cancel date range metrics should be returned")]
    public async Task AssertCancelDateRangeMetrics(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;

        var expectedMetrics = new CancelDateRangeResponse(
            int.Parse(cells.ElementAt(0).Value),
            int.Parse(cells.ElementAt(1).Value),
            int.Parse(cells.ElementAt(2).Value));

        StatusCode.Should().Be(HttpStatusCode.OK);
        CancelResponse.Should().BeEquivalentTo(expectedMetrics);
    }

    [When(@"I query for bookings for a person using their NHS number")]
    public async Task CheckAvailability()
    {
        var response = await GetHttpClientForTest().GetAsync($"http://localhost:7071/api/booking?nhsNumber={NhsNumber}");
        (_, BookingResponse) =
            await JsonRequestReader.ReadRequestAsync<List<Core.Bookings.Booking>>(
                await response.Content.ReadAsStreamAsync());
    }

    private async Task SendRequestAsync(object payload)
    {
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await GetHttpClientForTest().PostAsync("http://localhost:7071/api/availability/cancel-date-range", content);
        StatusCode = Response.StatusCode;
        (_, CancelResponse) =
            await JsonRequestReader.ReadRequestAsync<CancelDateRangeResponse>(
                await Response.Content.ReadAsStreamAsync());
    }
}

[Collection(FeatureToggleCollectionNames.CancelDateRangeAndBookingsCollection)]
[FeatureFile("./Scenarios/CancelDateRange/CanceDateRange_CancelDateRangeWithBookings_BothDisabled.feature")]
public class CanceDateRange_CancelDateRangeWithBookings_BothDisabled() : CancelDateRangeFeatureSteps([new FlagState(Flags.CancelADateRange, false), new FlagState(Flags.CancelADateRangeWithBookings, false)])
{
}

[Collection(FeatureToggleCollectionNames.CancelDateRangeAndBookingsCollection)]
[FeatureFile("./Scenarios/CancelDateRange/CancelDateRange_Disabled_CancelDateRangeWithBookings_Enabled.feature")]
public class CancelDateRange_Disabled_CancelDateRangeWithBookings_Enabled() : CancelDateRangeFeatureSteps([new FlagState(Flags.CancelADateRange, false), new FlagState(Flags.CancelADateRangeWithBookings, true)])
{
}

[Collection(FeatureToggleCollectionNames.CancelDateRangeAndBookingsCollection)]
[FeatureFile("./Scenarios/CancelDateRange/CancelDateRange_Enabled_CancelDateRangeWithBookings_Disabled.feature")]
public class CancelDateRange_Enabled_CancelDateRangeWithBookings_Disabled() : CancelDateRangeFeatureSteps([new FlagState(Flags.CancelADateRange, true), new FlagState(Flags.CancelADateRangeWithBookings, false)])
{
}

[Collection(FeatureToggleCollectionNames.CancelDateRangeAndBookingsCollection)]
[FeatureFile("./Scenarios/CancelDateRange/CancelDateRange_Enabled_CancelDateRangeWithBookings_Enabled.feature")]
public class CancelDateRange_Enabled_CancelDateRangeWithBookings_Enabled() : CancelDateRangeFeatureSteps([new FlagState(Flags.CancelADateRange, true), new FlagState(Flags.CancelADateRangeWithBookings, true)])
{
}
