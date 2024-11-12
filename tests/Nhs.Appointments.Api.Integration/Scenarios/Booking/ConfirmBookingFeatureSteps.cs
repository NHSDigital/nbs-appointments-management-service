using FluentAssertions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Persistance.Models;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking;

[FeatureFile("./Scenarios/Booking/ConfirmBooking.feature")]
public sealed class ConfirmBookingFeatureSteps : BookingBaseFeatureSteps
{
    [When("I confirm the booking")]
    public async Task ConfirmBooking()
    {
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking/{bookingReference}/confirm", new StringContent(""));
    }

    [When("the provisional bookings are cleaned up")]
    public async Task CleanUpProvisionalBookings()
    {
        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/system/run-provisional-sweep", new StringContent(""));
    }

    [When("I confirm the booking with the following contact information")]
    public async Task ConfirmBookingWithContactDetails(Gherkin.Ast.DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        var payload = new
        {
            contactDetails = new[]
            {
                new ContactItem("email", cells.ElementAt(0).Value),
                new ContactItem("phone", cells.ElementAt(1).Value),
                new ContactItem("landline", cells.ElementAt(2).Value)
            }
        };
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking/{bookingReference}/confirm", payload);
    }

    [Then("the call should be successful")]
    public void AssertHttpOk()
    {
        Response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }    

    [And("the booking is no longer marked as provisional")]
    public async Task AssertBookingNotProvisional()
    {
        var siteId = GetSiteId();
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        var actualBooking = await Client.GetContainer("appts", "booking_data").ReadItemAsync<BookingDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey(siteId));
        actualBooking.Resource.Provisional.Should().BeFalse();
    
        var actualBookingIndex = await Client.GetContainer("appts", "index_data").ReadItemAsync<BookingIndexDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey("booking_index"));
        actualBookingIndex.Resource.Provisional.Should().BeFalse();
    }

    [And("the booking should have stored my contact details as follows")]
    public async Task AssertBookingContactDetails(Gherkin.Ast.DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        var expectedContactDetails = new[]
        {
            new ContactItem("email", cells.ElementAt(0).Value),
            new ContactItem("phone", cells.ElementAt(1).Value),
            new ContactItem("landline", cells.ElementAt(2).Value)
        };

        var siteId = GetSiteId();
        var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Provisional);
        var actualBooking = await Client.GetContainer("appts", "booking_data").ReadItemAsync<BookingDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey(siteId));
        actualBooking.Resource.ContactDetails.Should().BeEquivalentTo(expectedContactDetails);
    }

    [And("the number of bookings removed should be in the response")]
    public async Task AssertNumberOfExpiredProvisionalBookingsRemoved()
    {
        var response = await Response.Content.ReadAsAsync<RemoveExpiredProvisionalBookingsResponse>();
        response.NumberRemoved.Should().NotBeNull();
    }
}

