using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Location;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

[FeatureFile("./Scenarios/SiteManagement/UpdateSiteReferenceDetails.feature")]
public sealed class UpdateSiteReferenceDetailsFeatureSteps : SiteManagementBaseFeatureSteps
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
