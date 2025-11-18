using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [Collection("BookingReferenceV2SerialToggle")]
    [FeatureFile("./Scenarios/Booking/MakeBooking.feature")]
    public class MakeBooking_BookingReferenceV2Enabled()
        : BookingBaseFeatureSteps(Flags.BookingReferenceV2, true);

    [Collection("BookingReferenceV2SerialToggle")]
    [FeatureFile("./Scenarios/Booking/MakeBooking.feature")]
    public class MakeBooking_BookingReferenceV2Disabled() 
        : BookingBaseFeatureSteps(Flags.BookingReferenceV2, false);
}
