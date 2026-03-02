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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Reports.SiteUsers;

[Collection(FeatureToggleCollectionNames.ReportsUpliftCollection)]
[FeatureFile("./Scenarios/Reports/SiteUsers/GetSiteUsersReport_Enabled.feature")]
public class GetSiteUsersReportFeatureSteps_Enabled()
    : GetSiteUsersReportFeatureSteps(Flags.ReportsUplift, true);

[Collection(FeatureToggleCollectionNames.ReportsUpliftCollection)]
[FeatureFile("./Scenarios/Reports/SiteUsers/GetSiteUsersReport_Disabled.feature")]
public class GetSiteUsersReportFeatureSteps_Disabled()
    : GetSiteUsersReportFeatureSteps(Flags.ReportsUplift, false);

public abstract class GetSiteUsersReportFeatureSteps(string flag, bool enabled) : SingleFeatureToggledSteps(flag, enabled), IAsyncLifetime
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

    [When("I request a site users report")]
    public async Task RequestSiteUsersReport()
    {
        var url = $"http://localhost:7071/api/report/sites/users";

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

    [And(@"the following role assignments for '(.+)' exist at site '(.+)'")]
    public async Task AddRoleAssignments(string user, string site, DataTable dataTable)
    {
        var roleAssignments = dataTable.Rows.Skip(1).Select(
            row => new RoleAssignment()
            {
                Scope = $"site:{site}",
                Role = row.Cells.ElementAt(0).Value
            }).ToArray();
        var userDocument = new UserDocument()
        {
            Id = GetUserId($"{user}_{site}"),
            DocumentType = "user",
            RoleAssignments = roleAssignments
        };
        await CosmosWrite(CosmosWriteAction.Create, "core_data", userDocument);
    }

    [And("the report contains the following data")]
    public void AssertReportData(DataTable dataTable)
    {
        var rows = dataTable.Rows.Skip(1);

        var textReader = new StringReader(ReportContent);
        var csvReader = new CsvReader(textReader,
            new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = "," });
        var actualData = csvReader.GetRecords<UserReportRow>();

        var expectedRows = rows.Select(r => new UserReportRow
        {
            Name = GetUserId(r.Cells.First().Value)
        });

        var actualReport = actualData.ToList();
        foreach (var expectedRow in expectedRows)
        {
            actualReport.Any(r => r.Name == expectedRow.Name).Should().BeTrue();
        }
    }

    private class UserReportRow
    {
        [Name("User")]
        public string Name { get; set; }
    }
}
