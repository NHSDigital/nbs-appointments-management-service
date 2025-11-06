using Nhs.Appointments.Api.Integration.Scenarios.ChangeSessionUpliftedJourney;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Reports.SiteSummary;

[Collection("SiteSummaryReportToggle")]
[FeatureFile("./Scenarios/Reports/SiteSummary/GetSiteSummaryReport_Enabled.feature")]
public class GetSiteSummaryReportFeatureSteps_Enabled()
    : ChangeSessionUpliftedJourneyFeatureSteps(Flags.SiteSummaryReport, true);

[Collection("SiteSummaryReportToggle")]
[FeatureFile("./Scenarios/Reports/SiteSummary/GetSiteSummaryReport_Disabled.feature")]
public class GetSiteSummaryReportFeatureSteps_Disabled()
    : ChangeSessionUpliftedJourneyFeatureSteps(Flags.SiteSummaryReport, false);

public class GetSiteSummaryReportFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{

}
