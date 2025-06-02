using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.BulkImport;

[FeatureFile("./Scenarios/BulkImport/UserBulkImport_Disabled.feature")]
public abstract class UserBulkImportDisabledFeatureSteps(string flag, bool enabled) : BaseBulkImportFeatureSteps(flag, enabled);

[Collection("BulkImportSerialToggle")]
public class UserBulkImport_BulkImportDisabled() : UserBulkImportDisabledFeatureSteps(Flags.BulkImport, false);

[Collection("BulkImportSerialToggle")]
public class UserBulkImport_BulkImportEnabled() : UserBulkImportEnabledFeatureSteps(Flags.BulkImport, true);

[FeatureFile("./Scenarios/BulkImport/UserBulkImport_Enabled.feature")]
public abstract class UserBulkImportEnabledFeatureSteps(string flag, bool enabled) : BaseBulkImportFeatureSteps(flag, enabled);
