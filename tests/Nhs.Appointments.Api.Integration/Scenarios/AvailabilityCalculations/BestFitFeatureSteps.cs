using FluentAssertions;
using Gherkin.Ast;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Json;
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

public abstract class BestFitFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private List<Core.Booking> _getBookingsResponse;

    [When("I cancel the following sessions")]
    [Then("I cancel the following sessions")]
    public async Task CancelSession(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows.Skip(1))
        {
            var cells = row.Cells.ToList();
            var date = DateTime.ParseExact(
                ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value).ToString("yyyy-MM-dd"),
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

            _response = await Http.PostAsJsonAsync("http://localhost:7071/api/session/edit", payload);
        }
    }

    [When(@"I query the current bookings")]
    public async Task CheckAvailability()
    {
        _response = await Http.GetAsync($"http://localhost:7071/api/booking?nhsNumber={NhsNumber}");
        _response.StatusCode.Should().Be(HttpStatusCode.OK);

        (_, _getBookingsResponse) =
            await JsonRequestReader.ReadRequestAsync<List<Core.Booking>>(await _response.Content.ReadAsStreamAsync());
    }

    [Then(@"the following bookings are returned")]
    public void Assert(DataTable expectedBookingDetailsTable)
    {
        var bookingIndex = 0;
        foreach (var row in expectedBookingDetailsTable.Rows.Skip(1))
        {
            var expectedBookingReference = _bookingReferences[bookingIndex];

            var expectedFrom = DateTime.ParseExact(
                $"{ParseNaturalLanguageDateOnly(row.Cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {row.Cells.ElementAt(1).Value}",
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

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => _response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Collection("ChangeSessionUpliftedJourneyToggle")]
    [FeatureFile("./Scenarios/AvailabilityCalculations/BestFit_ChangeSessionUpliftEnabled.feature")]
    public class BestFitFeatureSteps_ChangeSessionUplift_Enabled() : BestFitFeatureSteps(Flags.ChangeSessionUpliftedJourney, true);

    [Collection("ChangeSessionUpliftedJourneyToggle")]
    [FeatureFile("./Scenarios/AvailabilityCalculations/BestFit_ChangeSessionUpliftDisabled.feature")]
    public class BestFitFeatureSteps_ChangeSessionUplift_Disabled() : BestFitFeatureSteps(Flags.ChangeSessionUpliftedJourney, false);
}
