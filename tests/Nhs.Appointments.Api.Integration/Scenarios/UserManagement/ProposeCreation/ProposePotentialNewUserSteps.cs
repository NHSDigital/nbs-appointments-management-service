using Nhs.Appointments.Core.Features;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement.ProposeCreation;

[FeatureFile("./Scenarios/UserManagement/ProposeCreation/ProposePotentialNewUser_OktaDisabled.feature")]
public class ProposePotentialNewUserSteps_OktaDisabled() : ProposeCreationBaseFeatureSteps(Flags.OktaEnabled, false);

[FeatureFile("./Scenarios/UserManagement/ProposeCreation/ProposePotentialNewUser_OktaEnabled.feature")]
public class ProposePotentialNewUserSteps_OktaEnabled() : ProposeCreationBaseFeatureSteps(Flags.OktaEnabled, true);
