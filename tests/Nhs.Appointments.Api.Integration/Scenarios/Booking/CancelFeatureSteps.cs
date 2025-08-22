using System.Threading.Tasks;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking;

[FeatureFile("./Scenarios/Booking/Cancel.feature")]
public class CancelFeatureSteps : BookingBaseFeatureSteps;
