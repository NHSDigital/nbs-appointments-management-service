using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;
using Xunit;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Location;

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
    private string ReportContent { get; set; }

    [When(@"I request a site summary report for the following dates")]
    public async Task RequestReport(DataTable dataTable)
    {
        var row = dataTable.Rows.Skip(1).Single();

        var startDate = dataTable.GetNaturalLanguageDateRowValueOrDefault(row, "Start Date");
        var endDate = dataTable.GetNaturalLanguageDateRowValueOrDefault(row, "End Date");

        var url =
            $"http://localhost:7071/api/report/site-summary?startDate={startDate.ToString("yyyy-MM-dd")}&endDate={endDate.ToString("yyyy-MM-dd")}";

        Response = await Http.GetAsync(url);
        ReportContent = await Response.Content.ReadAsStringAsync();
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Then(@"the call should be successful")]
    public void AssertSuccessCode() => Response.EnsureSuccessStatusCode();

    [And("the report has the following headers")]
    public void AssertHeaders(DataTable dataTable)
    {
        var csvHeaders = ReportContent.Split("\n")[0].Trim('\r').Split(",");

        var row = dataTable.Rows.First();
        foreach (var cell in row.Cells)
        {
            csvHeaders.Should().Contain(cell.Value);
        }
    }

    [Given("the following sites exist in the system")]
    public async Task SetUpSites(DataTable dataTable)
    {
        var sites = dataTable.Rows.Skip(1).Select(row => new SiteDocument
        {
            Id = GetSiteId(dataTable.GetRowValueOrDefault(row, "Id")),
            Name = dataTable.GetRowValueOrDefault(row, "Name", "Hull Road Pharmacy"),
            Address = dataTable.GetRowValueOrDefault(row, "Address", "123 Hull Road"),
            PhoneNumber = dataTable.GetRowValueOrDefault(row, "Phone Number", "0113 1111111"),
            OdsCode = dataTable.GetRowValueOrDefault(row, "Ods Code", "ABC01"),
            Region = dataTable.GetRowValueOrDefault(row, "Region", "R1"),
            IntegratedCareBoard = dataTable.GetRowValueOrDefault(row, "ICB", "ICB1"),
            InformationForCitizens = dataTable.GetRowValueOrDefault(row, "Citizen Info",
                "Door buzzer does not work."),
            DocumentType = "site",
            Accessibilities = ParseAccessibilities(dataTable.GetRowValueOrDefault(row, "Access needs")),
            Location = new Location("Point",
                new[]
                {
                    dataTable.GetDoubleRowValueOrDefault(row, "Longitude", -1.67382134),
                    dataTable.GetDoubleRowValueOrDefault(row, "Latitude", 55.79628754)
                }),
            Type = dataTable.GetRowValueOrDefault(row, "Type"),
            IsDeleted = dataTable.GetBoolRowValueOrDefault(row, "Deleted")
        });

        foreach (var site in sites)
        {
            await Client.GetContainer("appts", "core_data").UpsertItemAsync(site);
        }
    }

    [And("the following site reports exist in the system")]
    public async Task SetUpSiteSummaries(DataTable dataTable)
    {
        var sites = dataTable.Rows.Skip(1).Select(row =>
        {
            var rsvBookings = dataTable.GetIntRowValueOrDefault(row, "RSV:Adult Bookings", 40);
            var rsvOrphaned = dataTable.GetIntRowValueOrDefault(row, "RSV:Adult Orphaned", 20);
            var rsvRemaining = dataTable.GetIntRowValueOrDefault(row, "RSV:Adult Remaining", 40);

            return new DailySiteSummaryDocument
            {
                Id = GetSiteId(dataTable.GetRowValueOrDefault(row, "Site")),
                Date = dataTable.GetNaturalLanguageDateRowValueOrDefault(row, "Date"),
                Bookings = new Dictionary<string, int> { { "RSV:Adult", rsvBookings } },
                Orphaned = new Dictionary<string, int> { { "RSV:Adult", rsvOrphaned } },
                Cancelled = dataTable.GetIntRowValueOrDefault(row, "Cancelled", 3),
                RemainingCapacity = new Dictionary<string, int> { { "RSV:Adult", rsvRemaining } },
                MaximumCapacity = dataTable.GetIntRowValueOrDefault(row, "Max Capacity", 100),
                GeneratedAtUtc = DateTime.UtcNow,
                DocumentType = "daily-site-summary-report"
            };
        });

        foreach (var site in sites)
        {
            await Client.GetContainer("appts", "aggregated_data").UpsertItemAsync(site);
        }
    }

    // TODO: Finish these assertions
    [And("the report contains the following data for site '(.+)'")]
    public async Task SetUpSiteSummaries(string siteName, DataTable dataTable)
    {
        var csvHeaders = ReportContent.Split("\n")[0].Trim('\r').Split(",");

        var csvLines = ReportContent.Split('\n');
        var lineForSite = csvLines.Single(line => line.Contains(siteName)).Trim('\r');
        var csvDataForSite = lineForSite.Split(',');

        var headers = dataTable.Rows.First().Cells;
        var expectedData = dataTable.Rows.Last();

        foreach (var header in headers)
        {
            var columnIndex = Array.IndexOf(csvHeaders, header.Value);
            var csvDatum = csvDataForSite[columnIndex];

            var expectedDatum = dataTable.GetRowValueOrDefault(expectedData, header.Value);

            csvDatum.Should().Be(expectedDatum);
        }
    }
}
