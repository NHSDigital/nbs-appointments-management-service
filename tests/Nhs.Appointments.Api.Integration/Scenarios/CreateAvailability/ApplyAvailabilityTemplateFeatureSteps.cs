using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    
    [Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
    [FeatureFile("./Scenarios/CreateAvailability/ApplyAvailabilityTemplate.feature")]
    public class ApplyAvailabilityTemplateSingleServiceFeaturesSteps_LastUpdatedByEnabled() : BaseCreateAvailabilityFeatureSteps(Flags.AuditLastUpdatedBy, true);


    [Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
    [FeatureFile("./Scenarios/CreateAvailability/ApplyAvailabilityTemplate.feature")]
    public class ApplyAvailabilityTemplateSingleServiceFeatureSteps_LastUpdatedByDisabled() : BaseCreateAvailabilityFeatureSteps(Flags.AuditLastUpdatedBy, false);
}
