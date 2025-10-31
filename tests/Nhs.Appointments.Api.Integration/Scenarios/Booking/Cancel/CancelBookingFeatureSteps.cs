using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking.Cancel;


[Collection("BookingReferenceV2SerialToggle")]
[FeatureFile("./Scenarios/Booking/Cancel/Cancel.feature")]
public class CancelBooking_BookingReferenceV2Enabled()
    : CancelBookingBaseFeatureSteps(Flags.BookingReferenceV2, true);

[Collection("BookingReferenceV2SerialToggle")]
[FeatureFile("./Scenarios/Booking/Cancel/Cancel.feature")]
public class CancelBooking_BookingReferenceV2Disabled() 
    : CancelBookingBaseFeatureSteps(Flags.BookingReferenceV2, false);

[Collection("BookingReferenceV2SerialToggle")]
[FeatureFile("./Scenarios/Booking/Cancel/Autocancellation.feature")]
public class AutoCancelBooking_BookingReferenceV2Enabled()
    : CancelBookingBaseFeatureSteps(Flags.BookingReferenceV2, true);

[Collection("BookingReferenceV2SerialToggle")]
[FeatureFile("./Scenarios/Booking/Cancel/Autocancellation.feature")]
public class AutoCancelBooking_BookingReferenceV2Disabled() 
    : CancelBookingBaseFeatureSteps(Flags.BookingReferenceV2, false);
