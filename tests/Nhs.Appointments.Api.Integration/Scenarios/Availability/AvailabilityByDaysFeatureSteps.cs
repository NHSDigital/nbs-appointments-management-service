using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability
{
    [Collection("MultipleServicesSerialToggle")]
    [FeatureFile("./Scenarios/Availability/AvailabilityByDays.feature")]
    public class AvailabilityByDaysFeatureSteps_MultipleServicesEnabled()
        : AvailabilityBaseFeatureSteps(Flags.MultipleServices, true);

    [Collection("MultipleServicesSerialToggle")]
    [FeatureFile("./Scenarios/Availability/AvailabilityByDays.feature")]
    public class AvailabilityByDaysFeatureSteps_MultipleServicesDisabled()
        : AvailabilityBaseFeatureSteps(Flags.MultipleServices, false);
}
