using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Json;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.AvailabilityCalculations;
public abstract class RecalculationProposalFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private AvailabilityChangeProposalResponse _availabilityChangeProposalResponse;
   

    [When(@"I request the availability proposal for potential availability change")]
    public async Task RequestAvailabilityRecalculation(DataTable proposalSessions)
    {
        Session matcher = null;
        Session replacement = null;

        foreach (var row in proposalSessions.Rows.Skip(1))
        {
            var session = new Session
            {
                From = string.IsNullOrEmpty(row.Cells.ElementAt(3).Value) ? default : TimeOnly.Parse(row.Cells.ElementAt(3).Value),
                Until = string.IsNullOrEmpty(row.Cells.ElementAt(4).Value) ? default : TimeOnly.Parse(row.Cells.ElementAt(4).Value),
                Services = string.IsNullOrEmpty(row.Cells.ElementAt(5).Value) ? Array.Empty<string>() : row.Cells.ElementAt(5).Value.Split(','),
                SlotLength = string.IsNullOrEmpty(row.Cells.ElementAt(6).Value) ? 0 : int.Parse(row.Cells.ElementAt(6).Value),
                Capacity = string.IsNullOrEmpty(row.Cells.ElementAt(7).Value) ? 0 : int.Parse(row.Cells.ElementAt(7).Value),
            };

            if (row.Cells.ElementAt(0).Value == "Matcher") matcher = session;
            if (row.Cells.ElementAt(0).Value == "Replacement") replacement = session;
        }

        var firstRow = proposalSessions.Rows.Skip(1).FirstOrDefault();
        object sessionMatcherObj = matcher ?? (object)"*";
        var request = new
        {

            site = GetSiteId(),
            from = NaturalLanguageDate.Parse(firstRow?.Cells.ElementAt(1).Value ?? "Tomorrow").ToString("yyyy-MM-dd"),
            to = NaturalLanguageDate.Parse(firstRow?.Cells.ElementAt(2).Value ?? "Tomorrow").ToString("yyyy-MM-dd"),
            sessionMatcher = sessionMatcherObj,
            sessionReplacement = replacement
        };
        var serializerSettings = new JsonSerializerSettings
        {
            Converters = { new ShortTimeOnlyJsonConverter() },
        };
        var content = new StringContent(JsonConvert.SerializeObject(request, serializerSettings), Encoding.UTF8, "application/json");

        _response = await Http.PostAsync($"http://localhost:7071/api/availability/propose-edit", content);
        _response.StatusCode.Should().Be(HttpStatusCode.OK);

        (_, _availabilityChangeProposalResponse) =
            await JsonRequestReader.ReadRequestAsync<AvailabilityChangeProposalResponse>(await _response.Content.ReadAsStreamAsync());
    }

    [Then(@"the following count is returned")]
    public async Task AssertAvailabilityCount(DataTable expectedCounts)
    {
        var counts = new List<int>();

        foreach (var row in expectedCounts.Rows)
        {
            counts.Add(int.Parse(row.Cells.ElementAt(1).Value));
        }

        _availabilityChangeProposalResponse.SupportedBookingsCount.Should().Be(counts[0]);
        _availabilityChangeProposalResponse.UnsupportedBookingsCount.Should().Be(counts[1]);
    }

    [When(@"I request recalculation proposal endpoint")]
    public async Task CallRecalculation()
    {
        var request = new
        {
            site = GetSiteId(),
            from = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
            to = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
            sessionMatcher = new
            {
                from = "09:00",
                until = "17:00",
                services = new string[] { "RSV:Adult" },
                slotLength = 5,
                capacity = 2
            },
            sessionReplacement = new
            {
                from = "10:00",
                until = "14:00",
                services = new string[] { "RSV:Adult", "COVID_FLU:65+" },
                slotLength = 5,
                capacity = 2
            }
        };

        var serializerSettings = new JsonSerializerSettings
        {
            Converters = { new ShortTimeOnlyJsonConverter() },
        };
        var content = new StringContent(JsonConvert.SerializeObject(request, serializerSettings), Encoding.UTF8, "application/json");

        _response = await Http.PostAsync($"http://localhost:7071/api/availability/propose-edit", content);
    }

    [Then(@"the call should fail with 404")]
    public async Task AssertNotFound()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Collection("RecalculationProposalToggle")]
    [FeatureFile("./Scenarios/AvailabilityCalculations/RecalculationProposal_Disabled.feature")]
    public class RecalculationProposalFeatureSteps_FeatureDisabled() : RecalculationProposalFeatureSteps(Flags.ChangeSessionUpliftedJourney, false);

    [Collection("RecalculationProposalToggle")]
    [FeatureFile("./Scenarios/AvailabilityCalculations/RecalculationProposal_Enabled.feature")]
    public class RecalculationProposalFeatureSteps_FeatureEnabled() : RecalculationProposalFeatureSteps(Flags.ChangeSessionUpliftedJourney, true);
}
