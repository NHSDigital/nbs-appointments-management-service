using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.AvailabilityCalculations;

[FeatureFile("./Scenarios/AvailabilityCalculations/BestFit.feature")]
public abstract class BestFitFeatureSteps(string flag, bool enabled) : BestFitBaseFeatureSteps(flag, enabled);

[Collection("MultipleServicesSerialToggle")]
public class BestFitFeatureSteps_MultipleServicesEnabled()
    : BestFitFeatureSteps(Flags.MultipleServices, true);

[Collection("MultipleServicesSerialToggle")]
public class BestFitFeatureSteps_MultipleServicesDisabled()
    : BestFitFeatureSteps(Flags.MultipleServices, false);
