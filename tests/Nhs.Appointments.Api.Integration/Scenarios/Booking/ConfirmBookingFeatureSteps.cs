using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking;

[FeatureFile("./Scenarios/Booking/ConfirmBooking.feature")]
public class ConfirmBookingFeatureSteps : BookingBaseFeatureSteps
{
    [When("I confirm the booking")]
    public async Task ConfirmBooking()
    {
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        var payload = new ConfirmBookingRequestPayload(
           contactDetails: [],
           relatedBookings: [],
           bookingToReschedule: string.Empty
       );
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await Http.PostAsync($"http://localhost:7071/api/booking/{bookingReference}/confirm", content);
    }

    [When("I confirm the bookings")]
    public async Task ConfirmBookings()
    {
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        var relatedBookingsReference = BookingReferences.GetBookingReference(1, BookingType.Provisional);
        var payload = new ConfirmBookingRequestPayload(
            contactDetails: [],
            relatedBookings: new[] { relatedBookingsReference },
            bookingToReschedule: "test-booking-to-reschedule"
        );
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Response = await Http.PostAsync($"http://localhost:7071/api/booking/{bookingReference}/confirm", content);
    }

    [When("the provisional bookings are cleaned up")]
    public async Task CleanUpProvisionalBookings()
    {
        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/system/run-provisional-sweep",
            new StringContent(""));
    }

    [When("I confirm the booking with the following contact information")]
    public async Task ConfirmBookingWithContactDetails(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;
        var payload = new
        {
            contactDetails = new[]
            {
                new { type = "Email", value = cells.ElementAt(0).Value },
                new { type = "Phone", value = cells.ElementAt(1).Value },
                new { type = "Landline", value = cells.ElementAt(2).Value }
            },
        };
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking/{bookingReference}/confirm", payload);
    }

    [When("I confirm bookings with the following contact information")]
    public async Task ConfirmBookingsWithContactDetails(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;
        var relatedBookingReference = BookingReferences.GetBookingReference(1, BookingType.Provisional);
        var payload = new
        {
            contactDetails = new[]
            {
                new { type = "Email", value = cells.ElementAt(0).Value },
                new { type = "Phone", value = cells.ElementAt(1).Value },
                new { type = "Landline", value = cells.ElementAt(2).Value }
            },
            relatedBookings = new[] { relatedBookingReference }
        };
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking/{bookingReference}/confirm", payload);
    }

    [Then("the call should be successful")]
    public void AssertHttpOk()
    {
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [And("the booking is no longer marked as provisional")]
    public async Task AssertBookingNotProvisional()
    {
        var siteId = GetSiteId();
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        var actualBooking = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
        actualBooking.Resource.Status.Should().Be(AppointmentStatus.Booked);

        var actualBookingIndex = await Client.GetContainer("appts", "index_data")
            .ReadItemAsync<BookingIndexDocument>(bookingReference, new PartitionKey("booking_index"));
        actualBookingIndex.Resource.Status.Should().Be(AppointmentStatus.Booked);
    }

    [And("the bookings are no longer marked as provisional")]
    public async Task AssertBookingsNotProvisional()
    {
        var siteId = GetSiteId();
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        var secondBookingReference = BookingReferences.GetBookingReference(1, BookingType.Provisional);
        var actualBooking = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
        var secondActualBooking = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(secondBookingReference, new PartitionKey(siteId));

        actualBooking.Resource.Status.Should().Be(AppointmentStatus.Booked);
        secondActualBooking.Resource.Status.Should().Be(AppointmentStatus.Booked);

        var actualBookingIndex = await Client.GetContainer("appts", "index_data")
            .ReadItemAsync<BookingIndexDocument>(bookingReference, new PartitionKey("booking_index"));
        var secondActualBookingIndex = await Client.GetContainer("appts", "index_data")
            .ReadItemAsync<BookingIndexDocument>(secondBookingReference, new PartitionKey("booking_index"));

        actualBookingIndex.Resource.Status.Should().Be(AppointmentStatus.Booked);
        secondActualBookingIndex.Resource.Status.Should().Be(AppointmentStatus.Booked);
    }

    [And("the booking should have stored my contact details as follows")]
    public async Task AssertBookingContactDetails(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        var expectedContactDetails = new[]
        {
            new ContactItem { Type = ContactItemType.Email, Value = cells.ElementAt(0).Value },
            new ContactItem { Type = ContactItemType.Phone, Value = cells.ElementAt(1).Value },
            new ContactItem { Type = ContactItemType.Landline, Value = cells.ElementAt(2).Value }
        };

        var siteId = GetSiteId();
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        var actualBooking = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
        actualBooking.Resource.ContactDetails.Should().BeEquivalentTo(expectedContactDetails);
    }

    [And("all bookings should have stored contact details as follows")]
    public async Task AssertBookingsContactDetails(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        var expectedContactDetails = new[]
        {
            new ContactItem { Type = ContactItemType.Email, Value = cells.ElementAt(0).Value },
            new ContactItem { Type = ContactItemType.Phone, Value = cells.ElementAt(1).Value },
            new ContactItem { Type = ContactItemType.Landline, Value = cells.ElementAt(2).Value }
        };

        var siteId = GetSiteId();
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        var secondBookingReference = BookingReferences.GetBookingReference(1, BookingType.Provisional);
        var actualBooking = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
        var secondActualBooking = await Client.GetContainer("appts", "booking_data")
            .ReadItemAsync<BookingDocument>(secondBookingReference, new PartitionKey(siteId));
        actualBooking.Resource.ContactDetails.Should().BeEquivalentTo(expectedContactDetails);
        secondActualBooking.Resource.ContactDetails.Should().BeEquivalentTo(expectedContactDetails);
    }
}
