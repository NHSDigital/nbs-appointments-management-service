using System.Linq;
using System.Threading.Tasks;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement;

public abstract class SiteManagementBaseFeatureSteps : BaseFeatureSteps
{
    [Given("The following sites exist in the system")]
    public async Task SetUpSites(Gherkin.Ast.DataTable dataTable)
    {
        var sites = dataTable.Rows.Skip(1).Select(
            row => new SiteDocument()
            {
                Id = row.Cells.ElementAt(0).Value,
                Name = row.Cells.ElementAt(1).Value,
                Address = row.Cells.ElementAt(2).Value,
                DocumentType = "site",
                AttributeValues = [new AttributeValue(row.Cells.ElementAt(3).Value, row.Cells.ElementAt(4).Value)],
                Location = new Location("Point", new[] { double.Parse(row.Cells.ElementAt(5).Value), double.Parse(row.Cells.ElementAt(6).Value) }),
            });
        
        foreach (var site in sites)
        {
            await Client.GetContainer("appts", "index_data").UpsertItemAsync(site);
        }
    }
    
}
