using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/ApplyAvailabilityTemplate.feature")]
    public abstract class ApplyAvailabilityTemplateFeatureSteps(string flag, bool enabled)
        : BaseCreateAvailabilityFeatureSteps(flag, enabled);

    [Collection("MultipleServicesSerialToggle")]
    public class ApplyAvailabilityTemplateFeatureSteps_MultipleServicesEnabled()
        : ApplyAvailabilityTemplateFeatureSteps(Flags.MultipleServices, true);

    [Collection("MultipleServicesSerialToggle")]
    public class ApplyAvailabilityTemplateFeatureSteps_MultipleServicesDisabled()
        : ApplyAvailabilityTemplateFeatureSteps(Flags.MultipleServices, false);
}
