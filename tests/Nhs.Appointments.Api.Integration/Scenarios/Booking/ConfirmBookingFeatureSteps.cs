using FluentAssertions;
using Nhs.Appointments.Persistance.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking;

[FeatureFile("./Scenarios/Booking/ConfirmBooking.feature")]
public sealed class ConfirmBookingFeatureSteps : BaseFeatureSteps
{
    private HttpResponseMessage _response;

    [When("I confirm the booking")]
    public async Task ConfirmBooking()
    {
        var bookingReference = GetBookingReference("0", BookingType.Provisional);
        _response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking/{bookingReference}/confirm", new StringContent(""));
    }

    [Then("the call should be successful")]
    public void AssertHttpOk()
    {
        _response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [And("the booking is no longer marked as provisional")]
    public async Task AssertBookingNotProvisional()
    {
        var siteId = GetSiteId();
        var bookingReference = GetBookingReference("0", BookingType.Provisional);
        var actualBooking = await Client.GetContainer("appts", "booking_data").ReadItemAsync<BookingDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey(siteId));
        actualBooking.Resource.Provisional.Should().BeFalse();

        var actualBookingIndex = await Client.GetContainer("appts", "index_data").ReadItemAsync<BookingIndexDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey("booking_index"));
        actualBookingIndex.Resource.Provisional.Should().BeFalse();
    }

    [When("the expired provisional bookings are swept")]
    public async Task RemovedExpiredProvisionalBooking()
    {
        var bookingReference = GetBookingReference("0", BookingType.Provisional);
        _response = await Http.PostAsJsonAsync($"http://localhost:7071/api/system/run-provisional-sweep", new StringContent(""));
    }

    [And("the booking no longer exists")]
    public async Task AssertBookingRemoved()
    {
        var siteId = GetSiteId();
        var bookingReference = GetBookingReference("0", BookingType.Provisional);
        var actualBooking = await Client.GetContainer("appts", "booking_data").ReadItemAsync<BookingDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey(siteId));
        actualBooking.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        var actualBookingIndex = await Client.GetContainer("appts", "index_data").ReadItemAsync<BookingIndexDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey("booking_index"));
        actualBookingIndex.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}

