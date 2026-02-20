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

public abstract class UpdateSiteReferenceDetailsFeatureSteps : SiteManagementBaseFeatureSteps
{
    [When("I update the reference details")]
    public async Task UpdateSiteReferenceDetails(DataTable dataTable)
    {
        var siteId = GetSiteId();
        var row = dataTable.Rows.ElementAt(1);
        
        var odsCode = row.Cells.ElementAt(0).Value;
        var icb = row.Cells.ElementAt(1).Value;
        var region = row.Cells.ElementAt(2).Value;
        
        var payload = new SetSiteReferenceDetailsRequest(siteId, odsCode, icb, region);
        Response = await GetHttpClientForTest().PostAsJsonAsync($"http://localhost:7071/api/sites/{siteId}/reference-details", payload);
    }
}

[FeatureFile("./Scenarios/SiteManagement/UpdateSiteReferenceDetails.feature")]
public sealed class UpdateSiteReferenceDetailsFeaturesSteps : UpdateSiteReferenceDetailsFeatureSteps;
