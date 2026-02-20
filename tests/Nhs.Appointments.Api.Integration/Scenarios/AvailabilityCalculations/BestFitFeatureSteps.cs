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
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Json;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.AvailabilityCalculations;

public abstract class BestFitFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private List<Core.Bookings.Booking> _getBookingsResponse;
    private AvailabilityChangeProposalResponse _availabilityChangeProposalResponse;

    [When("I cancel the following sessions at the default site")]
    [Then("I cancel the following sessions at the default site")]
    public async Task CancelSession(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows.Skip(1))
        {
            var cells = row.Cells.ToList();
            var date = DateTime.ParseExact(
                NaturalLanguageDate.Parse(cells.ElementAt(0).Value).ToString("yyyy-MM-dd"),
                "yyyy-MM-dd", null);

            object payload = new
            {
                site = GetSiteId(),
                from = DateOnly.FromDateTime(date),
                to = DateOnly.FromDateTime(date),
                sessionMatcher = new
                {
                    from = cells.ElementAt(1).Value,
                    until = cells.ElementAt(2).Value,
                    services = cells.ElementAt(3).Value.Split(',').Select(s => s.Trim()).ToArray(),
                    slotLength = int.Parse(cells.ElementAt(4).Value),
                    capacity = int.Parse(cells.ElementAt(5).Value)
                },
                sessionReplacement = null as Session
            };

            _response = await GetHttpClientForTest().PostAsJsonAsync("http://localhost:7071/api/session/edit", payload);
        }
    }

    [When(@"I query the current bookings")]
    public async Task CheckAvailability()
    {
        _response = await GetHttpClientForTest().GetAsync($"http://localhost:7071/api/booking?nhsNumber={NhsNumber}");
        _response.StatusCode.Should().Be(HttpStatusCode.OK);

        (_, _getBookingsResponse) =
            await JsonRequestReader.ReadRequestAsync<List<Core.Bookings.Booking>>(await _response.Content.ReadAsStreamAsync());
    }

    [Then(@"the following bookings are returned at the default site")]
    public void Assert(DataTable expectedBookingDetailsTable)
    {
        var bookingIndex = 0;
        foreach (var row in expectedBookingDetailsTable.Rows.Skip(1))
        {
            var expectedBookingReference = _getBookingsResponse[bookingIndex].Reference;

            var expectedFrom = DateTime.ParseExact(
                $"{NaturalLanguageDate.Parse(row.Cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {row.Cells.ElementAt(1).Value}",
                "yyyy-MM-dd HH:mm", null);
            var expectedDuration = int.Parse(row.Cells.ElementAt(2).Value);
            var expectedService = row.Cells.ElementAt(3).Value;
            var expectedAvailabilityStatus = Enum.Parse<AvailabilityStatus>(row.Cells.ElementAt(4).Value);

            var actualBooking = _getBookingsResponse.Single(booking => booking.Reference == expectedBookingReference);
            actualBooking.From.Should().Be(expectedFrom);
            actualBooking.Duration.Should().Be(expectedDuration);
            actualBooking.Service.Should().Be(expectedService);
            actualBooking.AvailabilityStatus.Should().Be(expectedAvailabilityStatus);

            bookingIndex += 1;
        }
    }

    [When(@"I request the availability proposal for potential availability change")]
    public async Task RequestAvailabilityRecalculation(DataTable proposalSessions)
    {
        var sessions = new List<Session>{ };

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

        var request = new { 
        
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

        _response = await GetHttpClientForTest().PostAsync($"http://localhost:7071/api/availability/propose-edit", content);
        _response.StatusCode.Should().Be(HttpStatusCode.OK);

        (_, _availabilityChangeProposalResponse) =
            await JsonRequestReader.ReadRequestAsync<AvailabilityChangeProposalResponse>(await _response.Content.ReadAsStreamAsync());
    }

    [Then(@"the following count is returned")]
    public void AssertAvailabilityCount(DataTable expectedCounts)
    {
        var counts = new List<int>();

        foreach (var row in expectedCounts.Rows)
        {
            counts.Add(int.Parse(row.Cells.ElementAt(1).Value));
        }

        _availabilityChangeProposalResponse.NewlySupportedBookingsCount.Should().Be(counts[0]);
        _availabilityChangeProposalResponse.NewlyUnsupportedBookingsCount.Should().Be(counts[1]);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => _response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Collection(FeatureToggleCollectionNames.ChangeSessionUpliftedJourneyCollection)]
    [FeatureFile("./Scenarios/AvailabilityCalculations/BestFit_ChangeSessionUpliftEnabled.feature")]
    public class BestFitFeatureSteps_ChangeSessionUplift_Enabled() : BestFitFeatureSteps(Flags.ChangeSessionUpliftedJourney, true);

    [Collection(FeatureToggleCollectionNames.ChangeSessionUpliftedJourneyCollection)]
    [FeatureFile("./Scenarios/AvailabilityCalculations/BestFit_ChangeSessionUpliftDisabled.feature")]
    public class BestFitFeatureSteps_ChangeSessionUplift_Disabled() : BestFitFeatureSteps(Flags.ChangeSessionUpliftedJourney, false);
}
