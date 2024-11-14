using System;
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
            actualBooking.Resource.Provisional.Should().BeFalse();

            var actualBookingIndex = await Client.GetContainer("appts", "index_data").ReadItemAsync<BookingIndexDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey("booking_index"));
            actualBookingIndex.Resource.Provisional.Should().BeFalse();
        }
        
        [And("I have an existing appointment less than 60 minutes from now")]
        public async Task SetupBookingLessThanOneHourAway()
        {
            var bookingDateTime = DateTime.Now.AddMinutes(59);
            var bookingDocument =  new BookingDocument
            {
                Id = BookingReferences.GetBookingReference(0, BookingType.Confirmed),
                DocumentType = "booking",
                Reference = BookingReferences.GetBookingReference(0, BookingType.Confirmed),
                From = bookingDateTime,
                Duration = 5,
                Service = "COVID",
                Site = GetSiteId(),
                Provisional = false,
                Created = DateTime.UtcNow,
                AttendeeDetails = new Core.AttendeeDetails
                {
                    NhsNumber = NhsNumber,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    DateOfBirth = new DateOnly(2000, 1, 1)
                }
            };
            var bookingIndexDocument = new BookingIndexDocument
                {
                    Reference = BookingReferences.GetBookingReference(0, BookingType.Confirmed),
                    Site = GetSiteId(),
                    DocumentType = "booking_index",
                    Id = BookingReferences.GetBookingReference(0, BookingType.Confirmed),
                    NhsNumber = NhsNumber,
                    Provisional = false,
                    Created = DateTime.UtcNow,
                    From = bookingDateTime,
                };
            await Client.GetContainer("appts", "booking_data").CreateItemAsync(bookingDocument);
            await Client.GetContainer("appts", "index_data").CreateItemAsync(bookingIndexDocument);
        }
        
    }
}
