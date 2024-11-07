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
    public sealed class CancelFeatureSteps : BookingBaseFeatureSteps
    {
        [When(@"I cancel the appointment")]
        public async Task CancelAppointment()
        {
            var payload = new
            {
                bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed),
                site = GetSiteId()
            };
            Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/booking/cancel", payload);
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
