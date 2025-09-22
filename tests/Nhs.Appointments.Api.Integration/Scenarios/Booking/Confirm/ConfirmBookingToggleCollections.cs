using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking.Confirm;

[Collection("ConfirmBookingSerialToggle")]
[FeatureFile("./Scenarios/Booking/Confirm/ConfirmBooking_JointBookings_Enabled.feature")]
public class ConfirmBooking_Enabled()
    : ConfirmBookingFeatureSteps(Flags.JointBookings, true);

[Collection("ConfirmBookingSerialToggle")]
[FeatureFile("./Scenarios/Booking/Confirm/ConfirmBooking_JointBookings_Disabled.feature")]
public class ConfirmBooking_Disabled()
    : ConfirmBookingFeatureSteps(Flags.JointBookings, false);
