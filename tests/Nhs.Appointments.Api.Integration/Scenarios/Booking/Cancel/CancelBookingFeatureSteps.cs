using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking.Cancel;

[FeatureFile("./Scenarios/Booking/Cancel/Cancel.feature")]
public class CancelBookingFeatureSteps : CancelBookingBaseFeatureSteps
{
}

[FeatureFile("./Scenarios/Booking/Cancel/Autocancellation.feature")]
public class CancelBookingAutocancellationFeatureSteps : CancelBookingBaseFeatureSteps
{
}
