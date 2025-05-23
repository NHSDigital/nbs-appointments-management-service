using System.Threading.Tasks;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Booking;

[FeatureFile("./Scenarios/Booking/Cancel_SingleService.feature")]
public abstract class CancelSingleServiceFeatureSteps(string flag, bool enabled)
    : BookingBaseFeatureSteps(flag, enabled);

[Collection("MultipleServicesSerialToggle")]
public class Cancel_SingleService_MultipleServicesEnabled()
    : CancelSingleServiceFeatureSteps(Flags.MultipleServices, true);

[Collection("MultipleServicesSerialToggle")]
public class Cancel_SingleService_MultipleServicesDisabled()
    : CancelSingleServiceFeatureSteps(Flags.MultipleServices, false);

[FeatureFile("./Scenarios/Booking/Cancel_MultipleServices.feature")]
[Collection("MultipleServicesSerialToggle")]
public class Cancel_MultipleServices()
    : BookingBaseFeatureSteps(Flags.MultipleServices, true);
