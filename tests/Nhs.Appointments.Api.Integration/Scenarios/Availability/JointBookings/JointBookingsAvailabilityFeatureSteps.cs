using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability.JointBookings;

[Collection(FeatureToggleCollectionNames.JointBookingsCollection)]
[FeatureFile("./Scenarios/Availability/JointBookings/JointBookings_Enabled.feature")]
public class JointBookings_Enabled()
    : AvailabilityFeatureToggledFeatureSteps(Flags.JointBookings, true);

[Collection(FeatureToggleCollectionNames.JointBookingsCollection)]
[FeatureFile("./Scenarios/Availability/JointBookings/JointBookings_Disabled.feature")]
public class JointBookings_Disabled() 
    : AvailabilityFeatureToggledFeatureSteps(Flags.JointBookings, false);
