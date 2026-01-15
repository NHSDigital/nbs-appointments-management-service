using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CreateAvailability
{
    [Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
    [FeatureFile("./Scenarios/CreateAvailability/SetAvailability.feature")]
    public sealed class SetAvailabilityFeaturesSteps_LastUpdatedByEnabled() : BaseCreateAvailabilityFeatureSteps(Flags.AuditLastUpdatedBy, true);


    [Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
    [FeatureFile("./Scenarios/CreateAvailability/SetAvailability.feature")]
    public sealed class SetAvailabilityFeatureSteps_LastUpdatedByDisabled() : BaseCreateAvailabilityFeatureSteps(Flags.AuditLastUpdatedBy, false);
}
