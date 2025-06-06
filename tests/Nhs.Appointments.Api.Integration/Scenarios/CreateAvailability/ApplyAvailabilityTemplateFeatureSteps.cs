using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [FeatureFile("./Scenarios/CreateAvailability/ApplyAvailabilityTemplate_SingleService.feature")]
    public abstract class ApplyAvailabilityTemplateSingleServiceFeatureSteps(string flag, bool enabled)
        : BaseCreateAvailabilityFeatureSteps(flag, enabled);

    [Collection("MultipleServicesSerialToggle")]
    public class ApplyAvailabilityTemplateFeatureSteps_SingleService_MultipleServicesEnabled()
        : ApplyAvailabilityTemplateSingleServiceFeatureSteps(Flags.MultipleServices, true);

    [Collection("MultipleServicesSerialToggle")]
    public class ApplyAvailabilityTemplateFeatureSteps_SingleService_MultipleServicesDisabled()
        : ApplyAvailabilityTemplateSingleServiceFeatureSteps(Flags.MultipleServices, false);
    
    [FeatureFile("./Scenarios/CreateAvailability/ApplyAvailabilityTemplate_MultipleServices.feature")]
    [Collection("MultipleServicesSerialToggle")]
    public class ApplyAvailabilityTemplateFeatureSteps_MultipleServices()
        : ApplyAvailabilityTemplateSingleServiceFeatureSteps(Flags.MultipleServices, true);
}
