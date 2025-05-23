using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking
{
    [FeatureFile("./Scenarios/Booking/MakeBooking_SingleService.feature")]
    public abstract class MakeBookingSingleServiceFeatureSteps(string flag, bool enabled)
        : BookingBaseFeatureSteps(flag, enabled);

    [Collection("MultipleServicesSerialToggle")]
    public class MakeBookingSingleService_MultipleServicesEnabled()
        : MakeBookingSingleServiceFeatureSteps(Flags.MultipleServices, true);
    
    [Collection("MultipleServicesSerialToggle")]
    public class MakeBookingSingleService_MultipleServicesDisabled()
        : MakeBookingSingleServiceFeatureSteps(Flags.MultipleServices, false);
    
    [FeatureFile("./Scenarios/Booking/MakeBooking_MultipleServices.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class MakeBookingMultipleServices()
        : BookingBaseFeatureSteps(Flags.MultipleServices, true);
}
