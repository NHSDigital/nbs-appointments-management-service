using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;

[Collection("MultipleServicesSerialToggle")]
[FeatureFile("./Scenarios/Availability/AvailabilityByHours.feature")]
public class AvailabilityByHoursFeatureSteps_MultipleServicesEnabled()
    : AvailabilityBaseFeatureSteps(Flags.MultipleServices, true);

[Collection("MultipleServicesSerialToggle")]
[FeatureFile("./Scenarios/Availability/AvailabilityByHours.feature")]
public class AvailabilityByHoursFeatureSteps_MultipleServicesDisabled()
    : AvailabilityBaseFeatureSteps(Flags.MultipleServices, false);

