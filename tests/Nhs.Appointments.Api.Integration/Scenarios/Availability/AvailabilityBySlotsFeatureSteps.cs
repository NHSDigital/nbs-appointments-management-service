using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;

[Collection("MultipleServicesSerialToggle")]
[FeatureFile("./Scenarios/Availability/AvailabilityBySlots_SingleService.feature")]
public class AvailabilityBySlotsFeatureSteps_SingleService_MultipleServicesEnabled()
    : AvailabilityBaseFeatureSteps(Flags.MultipleServices, true);

[Collection("MultipleServicesSerialToggle")]
[FeatureFile("./Scenarios/Availability/AvailabilityBySlots_SingleService.feature")]
public class AvailabilityBySlotsFeatureSteps_SingleService_MultipleServicesDisabled()
    : AvailabilityBaseFeatureSteps(Flags.MultipleServices, false);
    
[Collection("MultipleServicesSerialToggle")]
[FeatureFile("./Scenarios/Availability/AvailabilityBySlots_MultipleServices.feature")]
public class AvailabilityBySlotsFeatureSteps_MultipleServices()
    : AvailabilityBaseFeatureSteps(Flags.MultipleServices, true);
