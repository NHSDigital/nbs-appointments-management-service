using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;

[Collection("MultipleServicesSerialToggle")]
[FeatureFile("./Scenarios/Availability/AvailabilityBySlots.feature")]
public class AvailabilityBySlotsFeatureSteps_MultipleServicesEnabled() : AvailabilityBaseFeatureSteps(Flags.MultipleServices, true);

[Collection("MultipleServicesSerialToggle")]
[FeatureFile("./Scenarios/Availability/AvailabilityBySlots.feature")]
public class AvailabilityBySlotsFeatureSteps_MultipleServicesDisabled() : AvailabilityBaseFeatureSteps(Flags.MultipleServices, false);

