using Gherkin.Ast;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Features;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

public abstract class UpdateSiteAccessibilitiesFeatureSteps(string flag, bool enabled) : SiteManagementBaseFeatureSteps(flag, enabled)
{
    [When("I update the accessibilities for site '(.+)'")]
    public async Task UpdateSiteAccessibilities(string siteDesignation, DataTable dataTable)
    {
        var siteId = GetSiteId(siteDesignation);
        var row = dataTable.Rows.ElementAt(1);
        var accessibilities = ParseAccessibilities(row.Cells.ElementAt(0).Value);
        var payload = new SetSiteAccessibilitiesRequest(siteId, accessibilities);
        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/sites/{siteId}/accessibilities", payload);
    }
}

[Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
[FeatureFile("./Scenarios/SiteManagement/UpdateSiteAccessibilities.feature")]
public sealed class UpdateSiteAccessibilitiesFeaturesSteps_LastUpdatedByEnabled() : UpdateSiteAccessibilitiesFeatureSteps(Flags.AuditLastUpdatedBy, true);


[Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
[FeatureFile("./Scenarios/SiteManagement/UpdateSiteAccessibilities.feature")]
public sealed class UpdateSiteAccessibilitiesFeatureSteps_LastUpdatedByDisabled() : UpdateSiteAccessibilitiesFeatureSteps(Flags.AuditLastUpdatedBy, false);
