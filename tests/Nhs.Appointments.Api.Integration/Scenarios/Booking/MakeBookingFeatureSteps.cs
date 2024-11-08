using System;
using System.Linq;
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

        [When("I make a provisional appointment with the following details")]
        public async Task MakeProvisionalBooking(Gherkin.Ast.DataTable dataTable)
        {
            var cells = dataTable.Rows.ElementAt(1).Cells;

            object payload = new
            {
                from = cells.ElementAt(0).Value,
                duration = cells.ElementAt(1).Value,
                service = cells.ElementAt(2).Value,
                site = GetSiteId(),
                provisional = true,
                attendeeDetails = new
                {
                    nhsNumber = cells.ElementAt(3).Value,
                    firstName = cells.ElementAt(4).Value,
                    lastName = cells.ElementAt(5).Value,
                    dateOfBirth = cells.ElementAt(6).Value
                },
                addtionalData = new
                {
                    isAppBooking = cells.ElementAt(7).Value
                }
            };

            _response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking", payload);
        }

        [When("I make the appointment with the following details")]
        public async Task MakeBooking(Gherkin.Ast.DataTable dataTable)
        {
            var cells = dataTable.Rows.ElementAt(1).Cells;            

            object payload = new
            {
                from = cells.ElementAt(0).Value,
                duration = cells.ElementAt(1).Value,
                service = cells.ElementAt(2).Value,
                site = GetSiteId(),
                provisional = false,
                attendeeDetails = new
                {
                    nhsNumber = cells.ElementAt(3).Value,
                    firstName = cells.ElementAt(4).Value,
                    lastName = cells.ElementAt(5).Value,
                    dateOfBirth = cells.ElementAt(6).Value
                },
                contactDetails =
                    new[] {
                        new { type = "email", value = cells.ElementAt(7).Value },
                        new { type = "phone", value = cells.ElementAt(8).Value }
                    },
                additionalData = new
                {
                    isAppBooking = cells.ElementAt(9).Value
                }

            };
            _response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking", payload);
        }
        
        [Then(@"a reference number is returned containing '([\w:]+)' and the following booking is created")]
        public async Task Assert(string bookingIncrement, Gherkin.Ast.DataTable dataTable)
        {
            _response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var cells = dataTable.Rows.ElementAt(1).Cells;
            var siteId = GetSiteId();
            var result = JsonConvert.DeserializeObject<MakeBookingResponse>(await _response.Content.ReadAsStringAsync());
            var bookingReference = result.BookingReference;
            var isProvisional = cells.ElementAt(9).Value == "Yes";
            var expectedBooking = new BookingDocument()
            {
                Site = siteId,
                Reference = bookingReference,
                From = DateTime.ParseExact(cells.ElementAt(0).Value, "yyyy-MM-dd HH:mm", null),
                Duration = int.Parse(cells.ElementAt(1).Value),
                Service = cells.ElementAt(2).Value,
                Outcome = null,
                Created = DateTime.UtcNow,
                Provisional = isProvisional,
                AttendeeDetails = new AttendeeDetails()
                {
                    NhsNumber = cells.ElementAt(3).Value,
                    FirstName = cells.ElementAt(4).Value,
                    LastName = cells.ElementAt(5).Value,
                    DateOfBirth = DateOnly.ParseExact(cells.ElementAt(6).Value, "yyyy-MM-dd", null)
                },
                ContactDetails = isProvisional ? [] : 
                [
                    new ContactItem { Type = "email", Value = cells.ElementAt(7).Value },
                    new ContactItem { Type = "phone", Value = cells.ElementAt(8).Value }
                ],
                DocumentType = "booking",
                Id = bookingReference,
                AdditionalData = new
                {
                    isAppBooking = cells.ElementAt(10).Value
                }
            };
            
            result.BookingReference.Should().MatchRegex($"([0-9]){{2}}-([0-9]{{2}})-{bookingIncrement}");
            var actualBooking = await Client.GetContainer("appts", "booking_data").ReadItemAsync<BookingDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey(siteId));
            BookingAssertions.BookingsAreEquivalent(actualBooking, expectedBooking);
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
