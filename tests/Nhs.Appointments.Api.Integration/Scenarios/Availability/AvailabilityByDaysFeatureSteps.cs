using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability
{
    [Collection("MultipleServicesSerialToggle")]
    [FeatureFile("./Scenarios/Availability/AvailabilityByDays_SingleService.feature")]
    public class AvailabilityByDaysFeatureSteps_SingleService_MultipleServicesEnabled()
        : AvailabilityBaseFeatureSteps(Flags.MultipleServices, true);

    [Collection("MultipleServicesSerialToggle")]
    [FeatureFile("./Scenarios/Availability/AvailabilityByDays_SingleService.feature")]
    public class AvailabilityByDaysFeatureSteps_SingleService_MultipleServicesDisabled()
        : AvailabilityBaseFeatureSteps(Flags.MultipleServices, false);
    
    [Collection("MultipleServicesSerialToggle")]
    [FeatureFile("./Scenarios/Availability/AvailabilityByDays_MultipleServices.feature")]
    public class AvailabilityByDaysFeatureSteps_MultipleServices()
        : AvailabilityBaseFeatureSteps(Flags.MultipleServices, true);
}
