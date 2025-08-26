using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Availability.JointBookings;

[Collection("JointBookingsSerialToggle")]
[FeatureFile("./Scenarios/Availability/JointBookings/JointBookings_Enabled.feature")]
public class JointBookings_Enabled()
    : AvailabilityBaseFeatureSteps(Flags.JointBookings, true);

[Collection("JointBookingsSerialToggle")]
[FeatureFile("./Scenarios/Availability/JointBookings/JointBookings_Disabled.feature")]
public class JointBookings_Disabled()
    : AvailabilityBaseFeatureSteps(Flags.JointBookings, false);
