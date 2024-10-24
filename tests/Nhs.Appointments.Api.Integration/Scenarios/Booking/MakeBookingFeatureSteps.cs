using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using FluentAssertions;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Persistance.Models;
using AttendeeDetails = Nhs.Appointments.Core.AttendeeDetails;
using ContactItem = Nhs.Appointments.Core.ContactItem;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [FeatureFile("./Scenarios/Booking/MakeBooking.feature")]
    public sealed class MakeBookingFeatureSteps : BaseFeatureSteps
    {
        private  HttpResponseMessage _response;

        [When(@"I make the appointment for '([\w:]+)' at '(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2})'")]
        public Task MakeBooking(string service, string requestedAppointment)
        {
            return MakeBooking("A", service, requestedAppointment);
        }
        
        [When(@"I make the appointment at site '(\w)' for '([\w:]+)' at '(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2})'")]
        public async Task MakeBooking(string siteDesignation, string service, string requestedAppointment)
        {
            var payload = new
            {
                from = requestedAppointment,
                service = service,
                duration = 5,
                site = GetSiteId(siteDesignation),
                attendeeDetails = new { 
                    nhsNumber = "1234678890", 
                    firstName = "Bill", 
                    lastName = "Builder", 
                    dateOfBirth = "2000-02-01"
                },
                contactDetails = 
                     new[] { 
                         new { type = "email", value = "test@tempuri.org" },
                         new { type = "phone", value = "0123456789" }
                     }
                    
            };
            _response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking", payload);
        }
        
        [Then(@"the booking is created and the reference number is returned containing '([\w:]+)'")]
        public async Task Assert(string bookingIncrement)
        {
            var siteId = GetSiteId();
            var result = JsonConvert.DeserializeObject<MakeBookingResponse>(await _response.Content.ReadAsStringAsync());
            var bookingReference = result.BookingReference;
            var expectedBooking = new BookingDocument()
            {
                Site = siteId,
                Reference = bookingReference,
                From = new DateTime(2077, 01, 01, 9, 20, 0),
                Duration = 5,
                Service = "COVID",
                Outcome = null,
                AttendeeDetails = new AttendeeDetails()
                {
                    NhsNumber = "1234678890",
                    FirstName = "Bill",
                    LastName = "Builder",
                    DateOfBirth = new DateOnly(2000, 2, 1)
                },
                ContactDetails =
                [
                    new ContactItem { Type = "email", Value = "test@tempuri.org" },
                    new ContactItem { Type = "phone", Value = "0123456789" }
                ],
                DocumentType = "booking",
                Id = bookingReference
            };
            
            _response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            result.BookingReference.Should().MatchRegex($"([0-9]){{2}}-([0-9]{{2}})-{bookingIncrement}");
            var actualBooking = await Client.GetContainer("appts", "booking_data").ReadItemAsync<BookingDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey(siteId)); 
            actualBooking.Resource.Should().BeEquivalentTo(expectedBooking);
        }
        
        [Then(@"I receive a message informing me that the appointment is no longer available")]
        public async Task AssertBookingAppointmentGone()
        {
            _response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            var result = JsonConvert.DeserializeObject<ErrorResponseBody>(await _response.Content.ReadAsStringAsync());
            result.message.Should().Be("The time slot for this booking is not available");
        }
        
        public record ErrorResponseBody(string message, string property);
    }
}
