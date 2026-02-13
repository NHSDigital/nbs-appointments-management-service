using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Reports.MasterSiteList;

[Collection(FeatureToggleCollectionNames.ReportsUpliftCollection)]
[FeatureFile("./Scenarios/Reports/MasterSiteList/GetMasterSiteListReport_Enabled.feature")]
public class GetSiteUsersReportFeatureSteps_Enabled()
    : GetMasterSiteListReportFeatureSteps(Flags.ReportsUplift, true);

[Collection(FeatureToggleCollectionNames.ReportsUpliftCollection)]
[FeatureFile("./Scenarios/Reports/MasterSiteList/GetMasterSiteListReport_Disabled.feature")]
public class GetSiteUsersReportFeatureSteps_Disabled()
    : GetMasterSiteListReportFeatureSteps(Flags.ReportsUplift, false);

public abstract class GetMasterSiteListReportFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled), IAsyncLifetime
{
    private string ReportContent { get; set; }

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await CosmosDeleteFeed<SiteDocument>("core_data", sd => sd.Id.Contains(GetTestId), new PartitionKey("site"));
    }

    [When("I request master site list report")]
    public async Task RequestSiteUsersReport()
    {
        var url = $"http://localhost:7071/api/report/master-site-list";

        _response = await GetHttpClientForTest().GetAsync(url);
        _statusCode = _response.StatusCode;
        ReportContent = await _response.Content.ReadAsStringAsync();
    }

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

    [And("the report contains the following data")]
    public void AssertReportData(DataTable dataTable)
    {
        var rows = dataTable.Rows.Skip(1);

        var textReader = new StringReader(ReportContent);
        var csvReader = new CsvReader(textReader,
            new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = "," });
        var actualData = csvReader.GetRecords<MasterSiteListReportRow>();

        var expectedRows = rows.Select(r => new MasterSiteListReportRow
        {
            SiteName = r.Cells.ElementAt(0).Value,
            OdsCode = r.Cells.ElementAt(1).Value,
            SiteType = r.Cells.ElementAt(2).Value,
            Region = r.Cells.ElementAt(3).Value,
            ICB = r.Cells.ElementAt(4).Value,
            GUID = r.Cells.ElementAt(5).Value,
            IsDeleted = r.Cells.ElementAt(6).Value,
            Status = r.Cells.ElementAt(7).Value,
            Long = r.Cells.ElementAt(8).Value,
            Lat = r.Cells.ElementAt(9).Value,
            Address = r.Cells.ElementAt(10).Value
        });

        var actualReport = actualData.ToList();
        foreach (var expectedRow in expectedRows)
        {
            var realReport = actualReport.First(a => a.GUID.Contains(expectedRow.GUID));

            realReport.SiteName.Should().Be(expectedRow.SiteName);
            realReport.OdsCode.Should().Be(expectedRow.OdsCode);
            realReport.SiteType.Should().Be(expectedRow.SiteType);
            realReport.Region.Should().Be(expectedRow.Region);
            realReport.ICB.Should().Be(expectedRow.ICB);
            realReport.IsDeleted.Should().Be(expectedRow.IsDeleted);
            realReport.Status.Should().Be(expectedRow.Status);
            realReport.Lat.Should().Be(expectedRow.Lat);
            realReport.Long.Should().Be(expectedRow.Long);
            realReport.Address.Should().Be(expectedRow.Address);
        }
    }

    private class MasterSiteListReportRow
    {
        [Name("Site Name")]
        public string SiteName { get; set; }
        [Name("ODS Code")]
        public string OdsCode { get; set; }
        [Name("Site Type")]
        public string SiteType { get; set; }
        [Name("Region")]
        public string Region { get; set; }
        [Name("ICB")]
        public string ICB { get; set; }
        [Name("GUID")]
        public string GUID { get; set; }
        [Name("IsDeleted")]
        public string IsDeleted { get; set; }
        [Name("Status")]
        public string Status { get; set; }
        [Name("Long")]
        public string Long { get; set; }
        [Name("Lat")]
        public string Lat { get; set; }
        [Name("Address")]
        public string Address { get; set; }
    }
}
