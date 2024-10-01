using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using FluentAssertions;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Models;

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
                site = GetSiteId(siteDesignation),
                sessionHolder = "default", 
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
            var day = DateTime.Now.ToString("dd");
            _response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var result = JsonConvert.DeserializeObject<MakeBookingResponse>(await _response.Content.ReadAsStringAsync());
            result.BookingReference.Should().MatchRegex($"([0-9]){{2}}-([0-9]{{2}})-{bookingIncrement}");
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
