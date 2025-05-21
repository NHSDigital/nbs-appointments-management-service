using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/SetAvailability_MultipleServicesEnabled.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class SetAvailabilityFeatureSteps_MultipleServicesEnabled()
        : BaseCreateAvailabilityFeatureSteps(Flags.MultipleServices, true);

    [FeatureFile("./Scenarios/CreateAvailability/SetAvailability_MultipleServicesDisabled.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class SetAvailabilityFeatureSteps_MultipleServicesDisabled()
        : BaseCreateAvailabilityFeatureSteps(Flags.MultipleServices, false);
}
