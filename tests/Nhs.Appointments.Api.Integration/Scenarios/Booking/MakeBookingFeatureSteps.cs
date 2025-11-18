using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [Collection(FeatureToggleCollectionNames.BookingReferenceV2Collection)]
    [FeatureFile("./Scenarios/Booking/MakeBooking.feature")]
    public class MakeBooking_BookingReferenceV2Enabled()
        : BookingBaseFeatureSteps(Flags.BookingReferenceV2, true);

    [Collection(FeatureToggleCollectionNames.BookingReferenceV2Collection)]
    [FeatureFile("./Scenarios/Booking/MakeBooking.feature")]
    public class MakeBooking_BookingReferenceV2Disabled() 
        : BookingBaseFeatureSteps(Flags.BookingReferenceV2, false);
}
