using Gherkin.Ast;
using Nhs.Appointments.Api.Models;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/UpdateSiteDetails.feature")]
public sealed class UpdateSiteDetailsFeatureSteps : SiteManagementBaseFeatureSteps
{
    [When("I update the details for site '(.+)'")]
    public async Task UpdateSiteDetails(string siteDesignation, DataTable dataTable)
    {
        var siteId = GetSiteId(siteDesignation);
        var row = dataTable.Rows.ElementAt(1);
        
        var name = row.Cells.ElementAt(0).Value;
        var address = row.Cells.ElementAt(1).Value;
        var phoneNumber = row.Cells.ElementAt(2).Value;
        var latitude = row.Cells.ElementAt(3).Value;
        var longitude = row.Cells.ElementAt(4).Value;
        
        var payload = new SetSiteDetailsRequest(siteId, name, address, phoneNumber, latitude, longitude);
        Response = await Http.PostAsJsonAsync($"http://localhost:7071/api/sites/{siteId}/details", payload);
    }
}
