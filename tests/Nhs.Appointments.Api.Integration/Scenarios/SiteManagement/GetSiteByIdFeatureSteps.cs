using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Sites;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Sites.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

public abstract class GetSiteByIdFeatureSteps(string flag, bool enabled) : SiteManagementBaseFeatureSteps(flag, enabled)
{
    [When("I request site details for site '(.+)'")]
    public async Task RequestSites(string siteDesignation)
    {
        var siteId = GetSiteId(siteDesignation);
        Response = await Http.GetAsync($"http://localhost:7071/api/sites/{siteId}");
    }

    [Then("the correct site is returned")]
    public async Task AssertSite(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var expectedSite = new Site(
            Id: GetSiteId(row.Cells.ElementAt(0).Value),
            Name: row.Cells.ElementAt(1).Value,
            Address: row.Cells.ElementAt(2).Value,
            PhoneNumber: row.Cells.ElementAt(3).Value,
            OdsCode: row.Cells.ElementAt(4).Value,
            Region: row.Cells.ElementAt(5).Value,
            IntegratedCareBoard: row.Cells.ElementAt(6).Value,
            InformationForCitizens: row.Cells.ElementAt(7).Value,
            Accessibilities: ParseAccessibilities(row.Cells.ElementAt(8).Value),
            OutOfTheWayLocation,
            status: null, 
            isDeleted: dataTable.GetBoolRowValueOrDefault(row, "IsDeleted"),
            Type: dataTable.GetRowValueOrDefault(row, "Type")
        );
        Response.StatusCode.Should().Be(HttpStatusCode.OK);
        (_, ActualResponse) =
            await JsonRequestReader.ReadRequestAsync<Site>(await Response.Content.ReadAsStreamAsync());
        ActualResponse.Should().BeEquivalentTo(expectedSite, opts => opts.Excluding(x => x.LastUpdatedBy));
    }
}

[Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
[FeatureFile("./Scenarios/SiteManagement/GetSiteBySiteId.feature")]
public sealed class GetSiteByIdFeaturesSteps_LastUpdatedByEnabled() : GetSiteByIdFeatureSteps(Flags.AuditLastUpdatedBy, true);


[Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
[FeatureFile("./Scenarios/SiteManagement/GetSiteBySiteId.feature")]
public sealed class GetSiteByIdFeatureSteps_LastUpdatedByDisabled() : GetSiteByIdFeatureSteps(Flags.AuditLastUpdatedBy, false);
