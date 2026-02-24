using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [FeatureFile("./Scenarios/Booking/Reschedule.feature")]
    public class RescheduleFeatureSteps : BookingBaseFeatureSteps
    {
        [When("I extract the rescheduled booking reference")]
        [And("I extract the rescheduled booking reference")]
        public async Task ExtractRescheduledBookingReference()
        {
            var result = JsonConvert.DeserializeObject<MakeBookingResponse>(await _response.Content.ReadAsStringAsync());
            _reschduledBookingReference = result.BookingReference;
        }
        
        [When("I confirm the rescheduled booking")]
        [And("I confirm the rescheduled booking")]
        public async Task ConfirmBooking()
        {
            var bookingToReschedule = BookingReferences.GetBookingReference(0, BookingType.Confirmed);
            object payload = new
            {
                contactDetails =
                    new[]
                    {
                        new { type = "email", value = "test@test.com" },
                        new { type = "phone", value = "07777777777" }
                    },
                bookingToReschedule
            };
            _actionTimestamp = DateTimeOffset.UtcNow;
            _response = await GetHttpClientForTest().PostAsJsonAsync(
                $"http://localhost:7071/api/booking/{_reschduledBookingReference}/confirm", payload);
        }
        
        [Then("the rescheduled booking is no longer marked as provisional at the default site")]
        public async Task AssertRescheduledBookingIsNotProvisional()
        {
            var siteId = GetSiteId();
            var bookingReference = _reschduledBookingReference;

            var actualBooking = await CosmosReadItem<BookingDocument>("booking_data", bookingReference, new PartitionKey(siteId), CancellationToken.None);
            actualBooking.Resource.Status.Should().Be(AppointmentStatus.Booked);
            
            var bookingIndex = await CosmosReadItem<BookingIndexDocument>("index_data", bookingReference, new PartitionKey("booking_index"));
            bookingIndex.Resource.Status.Should().Be(AppointmentStatus.Booked);
        }
    }
}
