using System.Linq;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using DataTable = Gherkin.Ast.DataTable;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Api.Integration.Scenarios.Eula;

public abstract class BaseEulaFeatureSteps : BaseFeatureSteps
{
    [Given(@"the latest EULA is as follows")]
    public async Task SeedLatestEula(DataTable dataTable)
    {
        var cells = dataTable.Rows.Skip(1).Single().Cells;

        var versionDate = ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value);
        var eulaVersion = new EulaDocument()
        {
            Id = "eula",
            DocumentType = "eula",
            VersionDate = versionDate
        };

        await Client.GetContainer("appts", "index_data").UpsertItemAsync(eulaVersion);
    }
}