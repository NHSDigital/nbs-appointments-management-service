using FluentAssertions;
using Gherkin.Ast;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.AvailabilityCalculations;
public abstract class RecalculationProposalFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private AvailabilityChangeProposalResponse _availabilityChangeProposalResponse;
   

    [When(@"I request the availability proposal for potential availability change")]
    public async Task RequestAvailabilityRecalculation(DataTable proposalSessions)
    {
        var sessions = new List<Session> { };

        foreach (var row in proposalSessions.Rows.Skip(1))
        {
            var session = new Session
            {
                From = TimeOnly.Parse(row.Cells.ElementAt(0).Value),
                Until = TimeOnly.Parse(row.Cells.ElementAt(1).Value),
                Services = row.Cells.ElementAt(2).Value.Split(','),
                SlotLength = int.Parse(row.Cells.ElementAt(3).Value),
                Capacity = int.Parse(row.Cells.ElementAt(4).Value),
            };

            sessions.Add(session);
        }

        var request = new
        {

            site = GetSiteId(),
            from = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
            to = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
            sessionMatcher = new
            {
                from = sessions[0].From,
                until = sessions[0].Until,
                services = sessions[0].Services,
                slotLength = sessions[0].SlotLength,
                capacity = sessions[0].Capacity
            },
            sessionReplacement = new
            {
                from = sessions[1].From,
                until = sessions[1].Until,
                services = sessions[1].Services,
                slotLength = sessions[1].SlotLength,
                capacity = sessions[1].Capacity
            }
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

            },
            sessionReplacement = new
            {

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
