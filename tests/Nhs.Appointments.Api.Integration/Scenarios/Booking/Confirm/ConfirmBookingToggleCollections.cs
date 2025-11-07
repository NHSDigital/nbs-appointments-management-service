using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking.Confirm;

[Collection(FeatureToggleCollectionNames.JointBookingsCollection)]
[FeatureFile("./Scenarios/Booking/Confirm/ConfirmBooking_JointBookings_Enabled.feature")]
public class ConfirmBooking_Enabled()
    : ConfirmBookingFeatureSteps(Flags.JointBookings, true);

[Collection(FeatureToggleCollectionNames.JointBookingsCollection)]
[FeatureFile("./Scenarios/Booking/Confirm/ConfirmBooking_JointBookings_Disabled.feature")]
public class ConfirmBooking_Disabled()
    : ConfirmBookingFeatureSteps(Flags.JointBookings, false);
