using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.ReferenceNumber.V2;
using Nhs.Appointments.Persistance.Models;
using Xunit;
using Xunit.Gherkin.Quick;
using AttendeeDetails = Nhs.Appointments.Core.AttendeeDetails;
using ContactItem = Nhs.Appointments.Core.ContactItem;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking;

public abstract class BookingBaseFeatureSteps(string flag, bool enabled) : AuditFeatureSteps, IAsyncLifetime
{
    protected HttpResponseMessage Response { get; set; }
    
    [When(@"I cancel the appointment")]
    public async Task CancelAppointment()
    {
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);
        var site = GetSiteId();
        Response = await Http.PostAsync($"http://localhost:7071/api/booking/{bookingReference}/cancel?site={site}",
            null);
    }

    [When(@"I cancel the appointment with cancellation reason '(.+)'")]
    public async Task CancelAppointment(string cancellationReason)
    {
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);
        var site = GetSiteId();

        var payload = new { cancellationReason };

        var jsonContent = new StringContent(
            JsonConvert.SerializeObject(payload),
            Encoding.UTF8,
            "application/json"
        );

        Response = await Http.PostAsync($"http://localhost:7071/api/booking/{bookingReference}/cancel?site={site}", jsonContent);
    }

    [When(@"I cancel the appointment with reference '(.+)'")]
    public async Task CancelAppointmentWithReference(string reference)
    {
        var customId = CreateCustomBookingReference(reference);
        var site = GetSiteId();
        Response = await Http.PostAsync($"http://localhost:7071/api/booking/{customId}/cancel?site={site}", null);
    }
    
    [When("I make the appointment with the following details")]
    public async Task MakeBooking(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        object payload = new
        {
            from = DateTime.ParseExact(
                $"{ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value):yyyy-MM-dd} {cells.ElementAt(1).Value}",
                "yyyy-MM-dd HH:mm", null).ToString("yyyy-MM-dd HH:mm"),
            duration = cells.ElementAt(2).Value,
            service = cells.ElementAt(3).Value,
            site = GetSiteId(),
            kind = "booked",
            attendeeDetails = new
            {
                nhsNumber = cells.ElementAt(4).Value,
                firstName = cells.ElementAt(5).Value,
                lastName = cells.ElementAt(6).Value,
                dateOfBirth = cells.ElementAt(7).Value
            },
            contactDetails =
                new[]
                {
                    new { type = "email", value = cells.ElementAt(8).Value },
                    new { type = "phone", value = cells.ElementAt(9).Value },
                    new { type = "landline", value = cells.ElementAt(11).Value },
                },
            additionalData = new { isAppBooking = cells.ElementAt(10).Value }
        };
        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/booking", payload);
    }
    
    [Then(@"a reference number is returned and the following booking is created")]
    public async Task Assert(DataTable dataTable)
        {
            Response.StatusCode.Should().Be(HttpStatusCode.OK);
            var cells = dataTable.Rows.ElementAt(1).Cells;
            var siteId = GetSiteId();
            var result = JsonConvert.DeserializeObject<MakeBookingResponse>(await Response.Content.ReadAsStringAsync());
            var bookingReference = result.BookingReference;
            var isProvisional = cells.ElementAt(10).Value == "Yes";
            var expectedBooking = new BookingDocument
            {
                Site = siteId,
                Reference = bookingReference,
                From =
                    DateTime.ParseExact(
                        $"{ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value):yyyy-MM-dd} {cells.ElementAt(1).Value}",
                        "yyyy-MM-dd HH:mm", null),
                Duration = int.Parse(cells.ElementAt(2).Value),
                Service = cells.ElementAt(3).Value,
                Status = isProvisional ? AppointmentStatus.Provisional : AppointmentStatus.Booked,
                AvailabilityStatus = AvailabilityStatus.Supported,
                Created = DateTime.UtcNow,
                AttendeeDetails = new AttendeeDetails
                {
                    NhsNumber = cells.ElementAt(4).Value,
                    FirstName = cells.ElementAt(5).Value,
                    LastName = cells.ElementAt(6).Value,
                    DateOfBirth = DateOnly.ParseExact(cells.ElementAt(7).Value, "yyyy-MM-dd", null)
                },
                ContactDetails = isProvisional
                    ? []
                    :
                    [
                        new ContactItem { Type = ContactItemType.Email, Value = cells.ElementAt(8).Value },
                        new ContactItem { Type = ContactItemType.Phone, Value = cells.ElementAt(9).Value },
                        new ContactItem { Type = ContactItemType.Landline, Value = cells.ElementAt(12).Value }
                    ],
                DocumentType = "booking",
                Id = bookingReference,
                AdditionalData = new { isAppBooking = cells.ElementAt(11).Value }
            };
            
            ProviderHelper.IsValidBookingReference(result.BookingReference, null).Should().BeTrue();
            
            var actualBooking = await Client.GetContainer("appts", "booking_data")
                .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
            BookingAssertions.BookingsAreEquivalent(actualBooking, expectedBooking);
        }

    [Then(@"I receive a message informing me that the appointment is no longer available")]
    public async Task AssertBookingAppointmentGone()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var result = JsonConvert.DeserializeObject<ErrorResponseBody>(await Response.Content.ReadAsStringAsync());
        result.message.Should().Be("The time slot for this booking is not available");
    }

    public record ErrorResponseBody(string message, string property);
        
    [When("I make a provisional appointment with the following details")]
    public async Task MakeProvisionalBooking(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        object payload = new
        {
            from =
                DateTime.ParseExact(
                    $"{ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {cells.ElementAt(1).Value}",
                    "yyyy-MM-dd HH:mm", null).ToString("yyyy-MM-dd HH:mm"),
            duration = cells.ElementAt(2).Value,
            service = cells.ElementAt(3).Value,
            site = GetSiteId(),
            kind = "provisional",
            attendeeDetails = new
            {
                nhsNumber = EvaluateNhsNumber(cells.ElementAt(4).Value),
                firstName = cells.ElementAt(5).Value,
                lastName = cells.ElementAt(6).Value,
                dateOfBirth = cells.ElementAt(7).Value
            }
        };

        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/booking", payload);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    [And(@"default cancellation reason has been used")]
    public async Task AssertCancellationReason()
    {
        var expectedCancellationReason = CancellationReason.CancelledByCitizen;
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);

        await AssertCancellationReasonByReference(bookingReference, expectedCancellationReason);
    }

    [And(@"'(.+)' cancellation reason has been used")]
    public async Task AssertCancellationReason(string cancellationReason)
    {
        var expectedCancellationReason = Enum.Parse<CancellationReason>(cancellationReason);
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);

        await AssertCancellationReasonByReference(bookingReference, expectedCancellationReason);
    }

    protected string ToRequestFormat(string naturalLanguageDateOnly, string naturalLanguageTime)
    {
        return DateTime.ParseExact(
            $"{ParseNaturalLanguageDateOnly(naturalLanguageDateOnly):yyyy-MM-dd} {naturalLanguageTime}",
            "yyyy-MM-dd HH:mm", null).ToString("yyyy-MM-dd HH:mm");
    }
    
    private string Flag { get; } = flag;
    private bool Enabled { get; } = enabled;

    public async Task InitializeAsync()
    {
        await SetLocalFeatureToggleOverride(Flag, Enabled ? "True" : "False");
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}
