using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.BulkImport;

[Collection("BulkImportSerialToggle")]
[FeatureFile("./Scenarios/BulkImport/UserBulkImport_Disabled.feature")]
public class UserBulkImport_BulkImportDisabled() : BaseBulkImportFeatureSteps(Flags.BulkImport, false);

[Collection("BulkImportSerialToggle")]
[FeatureFile("./Scenarios/BulkImport/UserBulkImport_Enabled.feature")]
public class UserBulkImport_BulkImportEnabled() : BaseBulkImportFeatureSteps(Flags.BulkImport, true);
