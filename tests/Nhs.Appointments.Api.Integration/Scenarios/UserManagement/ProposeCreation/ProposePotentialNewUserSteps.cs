using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement.ProposeCreation;

[Collection("OktaEnabledSerialToggle")]
[FeatureFile("./Scenarios/UserManagement/ProposeCreation/ProposePotentialNewUser_OktaDisabled.feature")]
public class ProposePotentialNewUserSteps_OktaDisabled() : ProposeCreationBaseFeatureSteps(Flags.OktaEnabled, false);

[Collection("OktaEnabledSerialToggle")]
[FeatureFile("./Scenarios/UserManagement/ProposeCreation/ProposePotentialNewUser_OktaEnabled.feature")]
public class ProposePotentialNewUserSteps_OktaEnabled() : ProposeCreationBaseFeatureSteps(Flags.OktaEnabled, true);
