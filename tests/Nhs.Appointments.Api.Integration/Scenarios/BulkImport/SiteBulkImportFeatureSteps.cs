using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.BulkImport;

[FeatureFile("./Scenarios/BulkImport/SiteBulkImport_Enabled.feature")]
public abstract class SiteBulkImportEnabledFeatureSteps(string flag, bool enabled) : BaseBulkImportFeatureSteps(flag, enabled);

[Collection("BulkImportSeriaToggle")]
public class SiteBulkImport_BulkImportEnabled() : SiteBulkImportEnabledFeatureSteps(Flags.BulkImport, true);

[Collection("BulkImportSeriaToggle")]
public class SiteBulkImport_BulkImportDisabled() : SiteBulkImportDisabledFeatureSteps(Flags.BulkImport, false);

[FeatureFile("./Scenarios/BulkImport/SiteBulkImport_Disabled.feature")]
public class SiteBulkImportDisabledFeatureSteps(string flag, bool enabled) : BaseBulkImportFeatureSteps(flag, enabled);
