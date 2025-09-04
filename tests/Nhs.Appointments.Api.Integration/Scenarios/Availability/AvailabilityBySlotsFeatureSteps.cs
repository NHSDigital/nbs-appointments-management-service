using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability;

[FeatureFile("./Scenarios/Availability/AvailabilityBySlots.feature")]
public class AvailabilityBySlotsFeatureSteps : AvailabilityBaseFeatureSteps;
