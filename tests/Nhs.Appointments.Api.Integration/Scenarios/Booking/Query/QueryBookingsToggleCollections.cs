using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking.Query;

[Collection("QueryBookingsSerialToggle")]
[FeatureFile("./Scenarios/Booking/Query/QueryBookings_CancelDay_Enabled.feature")]
public class QueryBookings_CancelDayEnabled()
    : QueryBookingsFeatureSteps(Flags.CancelDay, true);

[Collection("QueryBookingsSerialToggle")]
[FeatureFile("./Scenarios/Booking/Query/QueryBookings_CancelDay_Disabled.feature")]
public class QueryBookings_CancelDayDisabled()
    : QueryBookingsFeatureSteps(Flags.CancelDay, false);
