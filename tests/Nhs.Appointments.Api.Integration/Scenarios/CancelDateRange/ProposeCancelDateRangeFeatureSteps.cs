using FluentAssertions;
using Gherkin.Ast;
using MassTransit.Internals.Caching;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Features;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CancelDateRange;

public abstract class ProposeCancelDateRangeFeatureSteps(string flag, bool enabled) : SingleFeatureToggledSteps(flag, enabled)
{
    private HttpResponseMessage Response { get; set; }
    private HttpStatusCode StatusCode { get; set; }
    private ProposeCancelDateRangeResponse ProposeResponse { get; set; }

    [When("I propose cancelling sessions and bookings for the default site within a date range")]
    public async Task ProposeCancelDateRange(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;

        var payload = new ProposeCancelDateRangeRequest(
            GetSiteId(),
            NaturalLanguageDate.Parse(cells.ElementAt(0).Value),
            NaturalLanguageDate.Parse(cells.ElementAt(1).Value));

        await SendRequestAsync(payload);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Then("the following proposed cancel a date range metrics should be returned")]
    public async Task AssertProposedCancelDateRangeMetrics(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).First();
        var cells = row.Cells;

        var expectedMetrics = new ProposeCancelDateRangeResponse(
            int.Parse(cells.ElementAt(0).Value),
            int.Parse(cells.ElementAt(1).Value));

        StatusCode.Should().Be(HttpStatusCode.OK);
        ProposeResponse.Should().BeEquivalentTo(expectedMetrics);
    }

    private async Task SendRequestAsync(object payload)
    {
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await GetHttpClientForTest().PostAsync("http://localhost:7071/api/availability/propose-cancel-date-range", content);
        StatusCode = Response.StatusCode;
        (_, ProposeResponse) =
            await JsonRequestReader.ReadRequestAsync<ProposeCancelDateRangeResponse>(
                await Response.Content.ReadAsStreamAsync());
    }
}

[Collection(FeatureToggleCollectionNames.CancelDateRangeAndBookingsCollection)]
[FeatureFile("./Scenarios/CancelDateRange/ProposeCancelDateRange_DIsabled.feature")]
public class ProposeCancelDateRange_Disabled() : ProposeCancelDateRangeFeatureSteps(Flags.CancelADateRange, false) { }


[Collection(FeatureToggleCollectionNames.CancelDateRangeAndBookingsCollection)]
[FeatureFile("./Scenarios/CancelDateRange/ProposeCancelDateRange_Enabled.feature")]
public class ProposeCancelDateRange_Enabled() : ProposeCancelDateRangeFeatureSteps(Flags.CancelADateRange, true) { }
