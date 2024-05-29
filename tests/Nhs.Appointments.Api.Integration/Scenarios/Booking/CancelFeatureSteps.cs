using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using FluentAssertions;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [FeatureFile("./Scenarios/Booking/Cancel.feature")]
    public sealed class CancelFeatureSteps : BaseFeatureSteps
    {
        private HttpResponseMessage _response;

        [When(@"I cancel the appointment")]
        public async Task CancelAppointment()
        {
            var payload = new
            {
                bookingReference = GetBookingReference(),
                site = GetSiteId()
            };
            _response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking/cancel", payload);
        }

        [Then(@"the appropriate booking has been '(\w+)'")]
        public async Task Assert(string outcome)
        {
            _response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var siteId = GetSiteId();
            var bookingReference = GetBookingReference();
            var actualResult = await Client.GetContainer("appts", "booking_data").ReadItemAsync<BookingDocument>(bookingReference, new Microsoft.Azure.Cosmos.PartitionKey(siteId));            
            actualResult.Resource.Outcome.Should().BeEquivalentTo(outcome);
        }

        private DayOfWeek[] ParseDays(string pattern)
        {
            if (pattern == "All")
                return new DayOfWeek[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };
            else
                return pattern.Split(",").Select(d => Enum.Parse(typeof(DayOfWeek), d, true)).Cast<DayOfWeek>().ToArray();
        }
    }
}
