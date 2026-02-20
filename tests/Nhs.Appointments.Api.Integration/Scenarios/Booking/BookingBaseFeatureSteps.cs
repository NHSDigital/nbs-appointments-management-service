using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using AttendeeDetails = Nhs.Appointments.Core.Bookings.AttendeeDetails;
using ContactItem = Nhs.Appointments.Core.Bookings.ContactItem;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking;

public abstract class BookingBaseFeatureSteps : AuditFeatureSteps
{
    protected HttpResponseMessage Response { get; set; }

    [When(@"I cancel the first booking without a site parameter")]
    public async Task CancelAppointment()
    {
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);
        Response = await GetHttpClientForTest().PostAsync($"http://localhost:7071/api/booking/{bookingReference}/cancel",
            null);
    }

    [When(@"I cancel the first confirmed booking at site '(.+)'")]
    public async Task CancelAppointmentAndProvideSite(string siteId)
    {
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);
        var site = GetSiteId(siteId);
        
        _actionTimestamp = DateTimeOffset.UtcNow;
        Response = await GetHttpClientForTest().PostAsync($"http://localhost:7071/api/booking/{bookingReference}/cancel?site={site}",
            null);
    }
    
    [When(@"I cancel the first confirmed booking at the default site")]
    public async Task CancelAppointmentDefault()
    {
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);
        var site = GetSiteId();
        
        _actionTimestamp = DateTimeOffset.UtcNow;
        Response = await GetHttpClientForTest().PostAsync($"http://localhost:7071/api/booking/{bookingReference}/cancel?site={site}",
            null);
    }

    [When(@"I cancel the first confirmed booking at the default site with cancellation reason '(.+)'")]
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

        Response = await GetHttpClientForTest().PostAsync($"http://localhost:7071/api/booking/{bookingReference}/cancel?site={site}",
            jsonContent);
    }

    [When(@"I cancel the booking at the default site with reference '(.+)'")]
    public async Task CancelAppointmentWithReference(string reference)
    {
        var customId = CreateUniqueTestValue(reference);
        var site = GetSiteId();
        Response = await GetHttpClientForTest().PostAsync($"http://localhost:7071/api/booking/{customId}/cancel?site={site}", null);
    }
    
    [When("I make the booking with the following details for the default site")]
    public async Task MakeBooking(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        object payload = new
        {
            from = DateTime.ParseExact(
                $"{NaturalLanguageDate.Parse(cells.ElementAt(0).Value):yyyy-MM-dd} {cells.ElementAt(1).Value}",
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
        
        _actionTimestamp = DateTimeOffset.UtcNow;
        Response = await GetHttpClientForTest().PostAsJsonAsync("http://localhost:7071/api/booking", payload);
    }

    [Then(@"a reference number is returned and the following booking is created at the default site")]
    public async Task AssertSingleBookingAtSite(DataTable dataTable)
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
                    $"{NaturalLanguageDate.Parse(cells.ElementAt(0).Value):yyyy-MM-dd} {cells.ElementAt(1).Value}",
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

        result.BookingReference.Should().MatchRegex("([0-9]){2}-([0-9]{2})-([0-9]{6})");
        
        var actualBooking = await CosmosReadItem<BookingDocument>("booking_data", bookingReference, new PartitionKey(siteId), CancellationToken.None);
        
        BookingAssertions.BookingsAreEquivalent(actualBooking, expectedBooking);
        
        await AssertLastUpdatedBy("booking_data", bookingReference, siteId, _userId);
    }
    
    [Then(@"I receive a message informing me that the booking is no longer available")]
    public async Task AssertBookingAppointmentGone()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var result = JsonConvert.DeserializeObject<ErrorResponseBody>(await Response.Content.ReadAsStringAsync());
        result.message.Should().Be("The time slot for this booking is not available");
    }
    
    [When("I make a provisional appointment with the following details at the default site")]
    public async Task MakeProvisionalBooking(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        object payload = new
        {
            from =
                DateTime.ParseExact(
                    $"{NaturalLanguageDate.Parse(cells.ElementAt(0).Value).ToString("yyyy-MM-dd")} {cells.ElementAt(1).Value}",
                    "yyyy-MM-dd HH:mm", null).ToString("yyyy-MM-dd HH:mm"),
            duration = cells.ElementAt(2).Value,
            service = cells.ElementAt(3).Value,
            site = GetSiteId(),
            kind = "Provisional",
            attendeeDetails = new
            {
                nhsNumber = EvaluateNhsNumber(cells.ElementAt(4).Value),
                firstName = cells.ElementAt(5).Value,
                lastName = cells.ElementAt(6).Value,
                dateOfBirth = cells.ElementAt(7).Value
            }
        };

        _actionTimestamp = DateTimeOffset.UtcNow;
        Response = await GetHttpClientForTest().PostAsJsonAsync("http://localhost:7071/api/booking", payload);
    }
    
    //Not to be used unless explicitly need to wait
    [When("I wait for '(.+)' milliseconds")]
    public async Task WaitForSeconds(string milliseconds)
    {
        var timespan = TimeSpan.FromMilliseconds(int.Parse(milliseconds));
        await Task.Delay(timespan);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    [And(@"the default cancellation reason has been used for the first booking at the default site")]
    public async Task AssertCancellationReason()
    {
        var expectedCancellationReason = CancellationReason.CancelledByCitizen;
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);

        await AssertCancellationReasonByReferenceAtDefaultSite(bookingReference, expectedCancellationReason);
    }
    
    [And(@"'(.+)' cancellation reason has been used for the first booking at the default site")]
    public async Task AssertCancellationReason(string cancellationReason)
    {
        var expectedCancellationReason = Enum.Parse<CancellationReason>(cancellationReason);
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);

        await AssertCancellationReasonByReferenceAtDefaultSite(bookingReference, expectedCancellationReason);
    }

    protected string ToRequestFormat(string naturalLanguageDateOnly, string naturalLanguageTime)
    {
        return DateTime.ParseExact(
            $"{NaturalLanguageDate.Parse(naturalLanguageDateOnly):yyyy-MM-dd} {naturalLanguageTime}",
            "yyyy-MM-dd HH:mm", null).ToString("yyyy-MM-dd HH:mm");
    }

    public record ErrorResponseBody(string message, string property);
}
