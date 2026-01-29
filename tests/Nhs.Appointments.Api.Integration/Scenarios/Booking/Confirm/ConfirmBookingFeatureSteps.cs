using System.Collections.Generic;
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
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using DataTable = Gherkin.Ast.DataTable;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking.Confirm;

public abstract class ConfirmBookingFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    [When("I confirm the booking")]
    public async Task ConfirmBooking()
    {
        var (url, payload) = BuildConfirmBookingPayload();

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        _response = await Http.PostAsync(url, content);
    }

    [When("I confirm the following bookings")]
    public async Task ConfirmBookingsTwo(DataTable dataTable)
    {
        var (url, payload) = BuildConfirmBookingPayload(dataTable);

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        _response = await Http.PostAsync(url, content);
    }
    
    [When("I confirm the following bookings for api user '(.+)'")]
    public async Task ConfirmBookingsTwoWithApiUser(string apiUser, DataTable dataTable)
    {
        var client = GetCustomHttpClient(apiUser);
        
        var (url, payload) = BuildConfirmBookingPayload(dataTable);

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        _response = await client.PostAsync(url, content);
    }

    [When("the provisional bookings are cleaned up")]
    public async Task CleanUpProvisionalBookings()
    {
        _response = await Http.PostAsJsonAsync("http://localhost:7071/api/system/run-provisional-sweep",
            new StringContent(""));
    }

    [Then("the call should be successful")]
    public void AssertHttpOk() => _response.StatusCode.Should().Be(HttpStatusCode.OK);

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

    [And("the following bookings are no longer marked as provisional")]
    public async Task AssertBookingsNotProvisional(DataTable dataTable)
    {
        var siteId = GetSiteId();
        var defaultReferenceOffset = 0;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var bookingReference = CreateCustomBookingReference(dataTable.GetRowValueOrDefault(row, "Reference")) ??
                                   BookingReferences.GetBookingReference(defaultReferenceOffset,
                                       BookingType.Provisional);
            defaultReferenceOffset += 1;


            var booking = await Client.GetContainer("appts", "booking_data")
                .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
            booking.Resource.Status.Should().Be(AppointmentStatus.Booked);

            var bookingIndex = await Client.GetContainer("appts", "index_data")
                .ReadItemAsync<BookingIndexDocument>(bookingReference, new PartitionKey("booking_index"));
            bookingIndex.Resource.Status.Should().Be(AppointmentStatus.Booked);
        }
    }

    [And("following bookings should have the following contact details")]
    public async Task AssertBookingsContactDetails(DataTable dataTable)
    {
        var siteId = GetSiteId();
        var defaultReferenceOffset = 0;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var bookingReference = CreateCustomBookingReference(dataTable.GetRowValueOrDefault(row, "Reference")) ??
                                   BookingReferences.GetBookingReference(defaultReferenceOffset,
                                       BookingType.Provisional);
            defaultReferenceOffset += 1;

            var expectedContactDetails = BuildExpectedContactItems(dataTable, row);

            var booking = await Client.GetContainer("appts", "booking_data")
                .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
            booking.Resource.ContactDetails.Should().BeEquivalentTo(expectedContactDetails);
        }
    }

    [And("following bookings should have the following batch size")]
    public async Task AssertBookingsBatchSize(DataTable dataTable)
    {
        var siteId = GetSiteId();
        var defaultReferenceOffset = 0;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var bookingReference = CreateCustomBookingReference(dataTable.GetRowValueOrDefault(row, "Reference")) ??
                                   BookingReferences.GetBookingReference(defaultReferenceOffset,
                                       BookingType.Provisional);
            defaultReferenceOffset += 1;

            var expectedBatchSize = dataTable.GetIntRowValueOrDefault(row, "Booking batch size", int.MinValue);

            var booking = await Client.GetContainer("appts", "booking_data")
                .ReadItemAsync<BookingDocument>(bookingReference, new PartitionKey(siteId));
            booking.Resource.BookingBatchSize.Should().Be(expectedBatchSize);
        }
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => _response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    private (string url, object payload) BuildConfirmBookingPayload(DataTable table)
    {
        var row = table.Rows.ElementAt(1);

        var primaryReference = CreateCustomBookingReference(table.GetRowValueOrDefault(row, "Reference")) ??
                               BookingReferences.GetBookingReference(0, BookingType.Provisional);

        var relatedBookings = table.GetListRowValueOrDefault(row, "Related bookings")
            ?.Select(CreateCustomBookingReference).ToArray();

        var bookingToReschedule = table.GetRowValueOrDefault(row, "Booking to Reschedule");

        var contactDetails = BuildRequestContactItems(table, row);

        var payload = new Dictionary<string, object>();
        if (relatedBookings != null)
        {
            payload["relatedBookings"] = relatedBookings;
        }

        if (bookingToReschedule != null)
        {
            payload["bookingToReschedule"] = bookingToReschedule;
        }

        if (contactDetails != null)
        {
            payload["contactDetails"] = contactDetails;
        }

        return ($"http://localhost:7071/api/booking/{primaryReference}/confirm", payload);
    }

    private (string url, object payload) BuildConfirmBookingPayload()
    {
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);

        var payload = new ConfirmBookingRequestPayload(
            [],
            [],
            string.Empty
        );

        return ($"http://localhost:7071/api/booking/{bookingReference}/confirm", payload);
    }

    private object[] BuildRequestContactItems(DataTable dataTable, TableRow row)
    {
        var email = dataTable.GetRowValueOrDefault(row, "Email");
        var phone = dataTable.GetRowValueOrDefault(row, "Phone");
        var landline = dataTable.GetRowValueOrDefault(row, "Landline");

        var details = new List<object>();
        if (!string.IsNullOrEmpty(email))
        {
            details.Add(new { Type = "Email", Value = email });
        }

        if (!string.IsNullOrEmpty(phone))
        {
            details.Add(new { Type = "Phone", Value = phone });
        }

        if (!string.IsNullOrEmpty(landline))
        {
            details.Add(new { Type = "Landline", Value = landline });
        }

        return details.ToArray();
    }

    private ContactItem[] BuildExpectedContactItems(DataTable dataTable, TableRow row)
    {
        var email = dataTable.GetRowValueOrDefault(row, "Email");
        var phone = dataTable.GetRowValueOrDefault(row, "Phone");
        var landline = dataTable.GetRowValueOrDefault(row, "Landline");

        var details = new List<ContactItem>();
        if (!string.IsNullOrEmpty(email))
        {
            details.Add(new ContactItem { Type = ContactItemType.Email, Value = email });
        }

        if (!string.IsNullOrEmpty(phone))
        {
            details.Add(new ContactItem { Type = ContactItemType.Phone, Value = phone });
        }

        if (!string.IsNullOrEmpty(landline))
        {
            details.Add(new ContactItem { Type = ContactItemType.Landline, Value = landline });
        }

        return details.ToArray();
    }
}
