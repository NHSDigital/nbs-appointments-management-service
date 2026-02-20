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

public abstract class UpdateSiteAccessibilitiesFeatureSteps : SiteManagementBaseFeatureSteps
{
    [When("I update the accessibilities for ODS code '(.+)'")]
    public async Task UpdateSiteAccessibilities(string odsCode, DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var accessibilities = ParseAccessibilities(row.Cells.ElementAt(0).Value);
        var payload = new SetSiteAccessibilitiesRequest(odsCode, accessibilities);
        Response = await GetHttpClientForTest().PostAsJsonAsync($"http://localhost:7071/api/sites/{odsCode}/accessibilities", payload);
    }
    
    [When("I update the accessibilities at the default site")]
    public async Task UpdateSiteAccessibilities(DataTable dataTable)
    {
        var siteId = GetSiteId();
        var row = dataTable.Rows.ElementAt(1);
        var accessibilities = ParseAccessibilities(row.Cells.ElementAt(0).Value);
        var payload = new SetSiteAccessibilitiesRequest(siteId, accessibilities);
        Response = await GetHttpClientForTest().PostAsJsonAsync($"http://localhost:7071/api/sites/{siteId}/accessibilities", payload);
    }
}

[FeatureFile("./Scenarios/SiteManagement/UpdateSiteAccessibilities.feature")]
public sealed class UpdateSiteAccessibilitiesFeaturesSteps: UpdateSiteAccessibilitiesFeatureSteps;
