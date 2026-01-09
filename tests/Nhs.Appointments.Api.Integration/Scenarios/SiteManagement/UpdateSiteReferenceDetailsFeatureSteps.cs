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

public abstract class UpdateSiteReferenceDetailsFeatureSteps(string flag, bool enabled) : SiteManagementBaseFeatureSteps(flag, enabled)
{
    [When("I update the reference details for site '(.+)'")]
    public async Task UpdateSiteReferenceDetails(string siteDesignation, DataTable dataTable)
    {
        var siteId = GetSiteId(siteDesignation);
        var row = dataTable.Rows.ElementAt(1);
        
        var odsCode = row.Cells.ElementAt(0).Value;
        var icb = row.Cells.ElementAt(1).Value;
        var region = row.Cells.ElementAt(2).Value;
        
        var payload = new SetSiteReferenceDetailsRequest(siteId, odsCode, icb, region);
        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/sites/{siteId}/reference-details", payload);
    }
}

[Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
[FeatureFile("./Scenarios/SiteManagement/UpdateSiteReferenceDetails.feature")]
public sealed class UpdateSiteReferenceDetailsFeaturesSteps_LastUpdatedByEnabled() : UpdateSiteReferenceDetailsFeatureSteps(Flags.AuditLastUpdatedBy, true);


[Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
[FeatureFile("./Scenarios/SiteManagement/UpdateSiteReferenceDetails.feature")]
public sealed class UpdateSiteReferenceDetailsFeatureSteps_LastUpdatedByDisabled() : UpdateSiteReferenceDetailsFeatureSteps(Flags.AuditLastUpdatedBy, false);
