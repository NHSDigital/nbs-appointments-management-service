using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.BulkImport;

[Collection("BulkImportSerialToggle")]
[FeatureFile("./Scenarios/BulkImport/SiteBulkImport_Enabled.feature")]
public class SiteBulkImport_BulkImportEnabled() : BaseBulkImportFeatureSteps(Flags.BulkImport, true);

[Collection("BulkImportSerialToggle")]
[FeatureFile("./Scenarios/BulkImport/SiteBulkImport_Disabled.feature")]
public class SiteBulkImport_BulkImportDisabled() : BaseBulkImportFeatureSteps(Flags.BulkImport, false);
