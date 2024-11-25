using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [FeatureFile("./Scenarios/Booking/Reschedule.feature")]
    public sealed class RescheduleFeatureSteps : BookingBaseFeatureSteps
    {
        private string _reschduledBookingReference;
        
        [And("I confirm the rescheduled booking")]
        public async Task ConfirmBooking()
        {
            var result = JsonConvert.DeserializeObject<MakeBookingResponse>(await Response.Content.ReadAsStringAsync());
            _reschduledBookingReference = result.BookingReference;
            var bookingToReschedule = BookingReferences.GetBookingReference(0, BookingType.Confirmed);
            object payload = new
            {
                contactDetails =
                    new[] {
                        new { type = "email", value = "test@test.com" },
                        new { type = "phone", value = "07777777777" }
                    },
                bookingToReschedule = bookingToReschedule
                
            };
            Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking/{_reschduledBookingReference}/confirm", payload);
        }
        
        [Then("the rescheduled booking is no longer marked as provisional")]
        public async Task AssertRescheduledBookingIsNotProvisional()
        {
            var siteId = GetSiteId();
            var bookingReference = _reschduledBookingReference;
            var actualBooking = await Client.GetContainer("appts", "booking_data").ReadItemAsync<BookingDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey(siteId));
            actualBooking.Resource.Status.Should().Be(Core.AppointmentStatus.Booked);

            var actualBookingIndex = await Client.GetContainer("appts", "index_data").ReadItemAsync<BookingIndexDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey("booking_index"));
            actualBookingIndex.Resource.Status.Should().Be(Core.AppointmentStatus.Booked);
        }
    }
}
