using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/SetAvailability.feature")]
    public abstract class SetAvailabilityFeatureSteps(string flag, bool enabled)
        : BaseCreateAvailabilityFeatureSteps(flag, enabled)
    {
    }

    [Collection("MultipleServicesSerialToggle")]
    public class SetAvailabilityFeatureSteps_MultipleServicesEnabled()
        : SetAvailabilityFeatureSteps(Flags.MultipleServices, true);

    [Collection("MultipleServicesSerialToggle")]
    public class SetAvailabilityFeatureSteps_MultipleServicesDisabled()
        : SetAvailabilityFeatureSteps(Flags.MultipleServices, false);
}
