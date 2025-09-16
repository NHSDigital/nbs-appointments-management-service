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
    private HttpResponseMessage _response;
    private List<Core.Booking> _getBookingsResponse;
    private AvailabilityChangeProposalResponse _availabilityChangeProposalResponse;
    private readonly Dictionary<int, string> _bookingReferences = new();

    [When(@"I create the following availability")]
    public async Task CreateAvailability(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows.Skip(1))
        {
            var cells = row.Cells.ToList();

            var date = cells.ElementAt(0).Value;
            var from = cells.ElementAt(1).Value;
            var until = cells.ElementAt(2).Value;
            var slotLength = cells.ElementAt(3).Value;
            var capacity = cells.ElementAt(4).Value;
            var services = cells.ElementAt(5).Value;

            var payload = new
            {
                date = ParseNaturalLanguageDateOnly(date).ToString("yyyy-MM-dd"),
                site = GetSiteId(),
                sessions = new[]
                {
                    new
                    {
                        from,
                        until,
                        slotLength = int.Parse(slotLength),
                        capacity = int.Parse(capacity),
                        services = services.Split(',').Select(s => s.Trim()).ToArray()
                    }
                },
                mode = "additive"
            };

            _response = await Http.PostAsJsonAsync("http://localhost:7071/api/availability", payload);
            _response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

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

    [Then("I make the following bookings")]
    public async Task MakeBookings(DataTable dataTable)
    {
        var bookingIndex = 0;
        foreach (var row in dataTable.Rows.Skip(1))
        {
            var cells = row.Cells.ToList();
            var date = cells.ElementAt(0).Value;
            var time = cells.ElementAt(1).Value;
            var duration = cells.ElementAt(2).Value;
            var service = cells.ElementAt(3).Value;

            object payload = new
            {
                from = DateTime.ParseExact(
                    $"{ParseNaturalLanguageDateOnly(date):yyyy-MM-dd} {time}",
                    "yyyy-MM-dd HH:mm", null).ToString("yyyy-MM-dd HH:mm"),
                duration,
                service,
                site = GetSiteId(),
                kind = "booked",
                attendeeDetails = new
                {
                    nhsNumber = NhsNumber, firstName = "John", lastName = "Doe", dateOfBirth = "1987-03-13"
                }
            };
            _response = await Http.PostAsJsonAsync("http://localhost:7071/api/booking", payload);
            _response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result =
                JsonConvert.DeserializeObject<MakeBookingResponse>(await _response.Content.ReadAsStringAsync());
            var bookingReference = result.BookingReference;

            _bookingReferences[bookingIndex] = bookingReference;
            bookingIndex += 1;
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

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => _response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Collection("ChangeSessionUpliftedJourneyToggle")]
    [FeatureFile("./Scenarios/AvailabilityCalculations/BestFit.feature")]
    public class BestFitFeatureSteps_ChangeSessionUplift_Enabled() : BestFitFeatureSteps(Flags.ChangeSessionUpliftedJourney, true);

    [Collection("ChangeSessionUpliftedJourneyToggle")]
    [FeatureFile("./Scenarios/AvailabilityCalculations/BestFit_ChangeSessionUpliftDisabled.feature")]
    public class BestFitFeatureSteps_ChangeSessionUplift_Disabled() : BestFitFeatureSteps(Flags.ChangeSessionUpliftedJourney, false);
}
