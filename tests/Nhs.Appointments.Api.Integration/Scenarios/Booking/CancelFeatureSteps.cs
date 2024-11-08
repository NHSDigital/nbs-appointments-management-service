using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [FeatureFile("./Scenarios/Booking/Cancel.feature")]
    public sealed class CancelFeatureSteps : BookingBaseFeatureSteps
    {
        [When(@"I cancel the appointment")]
        public async Task CancelAppointment()
        {
            var bookingReference = BookingReferences.GetBookingReference(0, BookingType.Confirmed);
            Response = await Http.PostAsync($"http://localhost:7071/api/booking/{bookingReference}/cancel", null);
        }
    }
}
