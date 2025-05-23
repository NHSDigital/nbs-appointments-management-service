using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/SetAvailability_SingleService_MultipleServicesEnabled.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class SetAvailability_SingleService_MultipleServicesEnabled()
        : BaseCreateAvailabilityFeatureSteps(Flags.MultipleServices, true);

    [FeatureFile("./Scenarios/CreateAvailability/SetAvailability_SingleService_MultipleServicesDisabled.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class SetAvailability_SingleService_MultipleServicesDisabled()
        : BaseCreateAvailabilityFeatureSteps(Flags.MultipleServices, false);
    
    [FeatureFile("./Scenarios/CreateAvailability/SetAvailability_MultipleServices.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class SetAvailability_MultipleServices()
        : BaseCreateAvailabilityFeatureSteps(Flags.MultipleServices, true);
}
