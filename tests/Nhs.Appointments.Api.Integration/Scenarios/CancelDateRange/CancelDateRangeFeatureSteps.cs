using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
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

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => StatusCode.Should().Be((HttpStatusCode)statusCode);

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
