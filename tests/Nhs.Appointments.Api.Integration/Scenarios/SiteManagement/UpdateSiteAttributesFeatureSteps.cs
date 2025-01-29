using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Gherkin.Ast;
using Nhs.Appointments.Api.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/UpdateSiteAttributes.feature")]
public sealed class UpdateSiteAttributesFeatureSteps : SiteManagementBaseFeatureSteps
{
    [When("I update the attributes for site '(.+)'")]
    public async Task UpdateSiteAttributes(string siteDesignation, DataTable dataTable)
    {
        var siteId = GetSiteId(siteDesignation);
        var row = dataTable.Rows.ElementAt(1);
        var attributeValues = ParseAccessibilities(row.Cells.ElementAt(0).Value);
        var payload = new SetSiteAttributesRequest(siteId, attributeValues);
        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/sites/{siteId}/attributes", payload);
    }
}
