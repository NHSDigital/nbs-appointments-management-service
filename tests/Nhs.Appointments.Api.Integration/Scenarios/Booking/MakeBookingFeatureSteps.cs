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
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [FeatureFile("./Scenarios/Booking/MakeBooking.feature")]
    public sealed class MakeBookingFeatureSteps : BookingBaseFeatureSteps
    {
        [When("I make the appointment with the following details")]
        public async Task MakeBooking(Gherkin.Ast.DataTable dataTable)
        {
            var cells = dataTable.Rows.ElementAt(1).Cells;

            object payload = new
            {
                from = DateTime.ParseExact(
                    $"{ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value):yyyy-MM-dd} {cells.ElementAt(1).Value}",
                    "yyyy-MM-dd HH:mm", null).ToString("yyyy-MM-dd HH:mm"),
                duration = cells.ElementAt(2).Value,
                service = cells.ElementAt(3).Value,
                site = GetSiteId(),
                provisional = false,
                attendeeDetails = new
                {
                    nhsNumber = cells.ElementAt(4).Value,
                    firstName = cells.ElementAt(5).Value,
                    lastName = cells.ElementAt(6).Value,
                    dateOfBirth = cells.ElementAt(7).Value
                },
                contactDetails =
                    new[] {
                        new { type = ContactItemType.Email, value = cells.ElementAt(8).Value },
                        new { type = ContactItemType.Phone, value = cells.ElementAt(9).Value },
                        new { type = ContactItemType.Landline, value = cells.ElementAt(11).Value },
                    },
                additionalData = new
                {
                    isAppBooking = cells.ElementAt(10).Value
                }

            };
            Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking", payload);
        }
        
        [Then(@"a reference number is returned and the following booking is created")]
        public async Task Assert(Gherkin.Ast.DataTable dataTable)
        {
            Response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var cells = dataTable.Rows.ElementAt(1).Cells;
            var siteId = GetSiteId();
            var result = JsonConvert.DeserializeObject<MakeBookingResponse>(await Response.Content.ReadAsStringAsync());
            var bookingReference = result.BookingReference;
            var isProvisional = cells.ElementAt(10).Value == "Yes";
            var expectedBooking = new BookingDocument()
            {
                Site = siteId,
                Reference = bookingReference,
                From = DateTime.ParseExact($"{ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value):yyyy-MM-dd} {cells.ElementAt(1).Value}", "yyyy-MM-dd HH:mm", null),
                Duration = int.Parse(cells.ElementAt(2).Value),
                Service = cells.ElementAt(3).Value,
                Outcome = null,
                Created = DateTime.UtcNow,
                Provisional = isProvisional,
                AttendeeDetails = new AttendeeDetails()
                {
                    NhsNumber = cells.ElementAt(4).Value,
                    FirstName = cells.ElementAt(5).Value,
                    LastName = cells.ElementAt(6).Value,
                    DateOfBirth = DateOnly.ParseExact(cells.ElementAt(7).Value, "yyyy-MM-dd", null)
                },
                ContactDetails = isProvisional ? [] : 
                [
                    new ContactItem { Type = ContactItemType.Email, Value = cells.ElementAt(8).Value },
                    new ContactItem { Type = ContactItemType.Phone, Value = cells.ElementAt(9).Value },
                    new ContactItem { Type = ContactItemType.Landline, Value = cells.ElementAt(12).Value }
                ],
                DocumentType = "booking",
                Id = bookingReference,
                AdditionalData = new
                {
                    isAppBooking = cells.ElementAt(11).Value
                }
            };
            
            result.BookingReference.Should().MatchRegex($"([0-9]){{2}}-([0-9]{{2}})-([0-9]{{6}})");
            var actualBooking = await Client.GetContainer("appts", "booking_data").ReadItemAsync<BookingDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey(siteId));
            BookingAssertions.BookingsAreEquivalent(actualBooking, expectedBooking);
        }
        
        [Then(@"I receive a message informing me that the appointment is no longer available")]
        public async Task AssertBookingAppointmentGone()
        {
            Response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
            var result = JsonConvert.DeserializeObject<ErrorResponseBody>(await Response.Content.ReadAsStringAsync());
            result.message.Should().Be("The time slot for this booking is not available");
        }
        
        public record ErrorResponseBody(string message, string property);        
    }    
}
