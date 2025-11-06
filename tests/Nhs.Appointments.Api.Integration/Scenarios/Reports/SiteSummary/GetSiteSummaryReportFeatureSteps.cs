using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Core.Features;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Reports.SiteSummary;

[Collection("SiteSummaryReportToggle")]
[FeatureFile("./Scenarios/Reports/SiteSummary/GetSiteSummaryReport_Enabled.feature")]
public class GetSiteSummaryReportFeatureSteps_Enabled()
    : GetSiteSummaryReportFeatureSteps(Flags.SiteSummaryReport, true);

[Collection("SiteSummaryReportToggle")]
[FeatureFile("./Scenarios/Reports/SiteSummary/GetSiteSummaryReport_Disabled.feature")]
public class GetSiteSummaryReportFeatureSteps_Disabled()
    : GetSiteSummaryReportFeatureSteps(Flags.SiteSummaryReport, false);

public abstract class GetSiteSummaryReportFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private HttpResponseMessage Response { get; set; }

    [When(@"I request a site summary report for the following dates")]
    public async Task RequestReport(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).Single();

        var startDate = dataTable.GetNaturalLanguageDateRowValueOrDefault(row, "Start Date");
        var endDate = dataTable.GetNaturalLanguageDateRowValueOrDefault(row, "End Date");

        var url =
            $"http://localhost:7071/api/report/site-summary?startDate={startDate.ToString("yyyy-MM-dd")}&endDate={endDate.ToString("yyyy-MM-dd")}";

        Response = await Http.GetAsync(url);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);
}
