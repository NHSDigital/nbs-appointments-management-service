using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;
using Xunit;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Sites.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios.Reports.SiteSummary;

[Collection(FeatureToggleCollectionNames.SiteSummaryReportCollection)]
[FeatureFile("./Scenarios/Reports/SiteSummary/GetSiteSummaryReport_Enabled.feature")]
public class GetSiteSummaryReportFeatureSteps_Enabled()
    : GetSiteSummaryReportFeatureSteps(Flags.SiteSummaryReport, true);

[Collection(FeatureToggleCollectionNames.SiteSummaryReportCollection)]
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
            await CosmosAction_RetryOnTooManyRequests(CosmosAction.Upsert, Client.GetContainer("appts", "aggregated_data"), site);
        }
    }

    [And("the report contains a row with the following data")]
    public void AssertRowExistence(DataTable dataTable)
    {
        var textReader = new StringReader(ReportContent);
        var csvReader = new CsvReader(textReader,
            new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = "," });
        var actualData = csvReader.GetRecords<SiteReportRow>();

        var row = dataTable.Rows.Last();

        var expectedRow = new SiteReportRow
        {
            SiteName = dataTable.GetRowValueOrDefault(row, "Name"),
            SiteType = dataTable.GetRowValueOrDefault(row, "Site Type"),
            Region = dataTable.GetRowValueOrDefault(row, "Region"),
            RegionName = dataTable.GetRowValueOrDefault(row, "Region Name"),
            Icb = dataTable.GetRowValueOrDefault(row, "ICB"),
            IcbName = dataTable.GetRowValueOrDefault(row, "ICB Name"),
            RsvBooked = dataTable.GetRowValueOrDefault(row, "RSV:Adult Booked"),
            RsvCapacity = dataTable.GetRowValueOrDefault(row, "RSV:Adult Capacity"),
            TotalBookings = dataTable.GetRowValueOrDefault(row, "Total Bookings"),
            Cancelled = dataTable.GetRowValueOrDefault(row, "Cancelled"),
            MaxCapacity = dataTable.GetRowValueOrDefault(row, "Max Capacity")
        };

        var actualReport = actualData.Single(d => d.SiteName == expectedRow.SiteName);
        actualReport.Should().BeEquivalentTo(expectedRow);
    }
}

public class SiteReportRow
{
    [Name("Site Name")] public string SiteName { get; set; }
    [Name("Site Type")] public string SiteType { get; set; }
    [Name("Region")] public string Region { get; set; }
    [Name("Region Name")] public string RegionName { get; set; }

    [Name("ICB")] public string Icb { get; set; }
    [Name("ICB Name")] public string IcbName { get; set; }

    [Name("RSV:Adult Booked")] public string RsvBooked { get; set; }
    [Name("Total Bookings")] public string TotalBookings { get; set; }
    [Name("Cancelled")] public string Cancelled { get; set; }
    [Name("Maximum Capacity")] public string MaxCapacity { get; set; }
    [Name("RSV:Adult Capacity")] public string RsvCapacity { get; set; }
}
