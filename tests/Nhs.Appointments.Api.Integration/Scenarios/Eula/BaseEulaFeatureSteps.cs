using System.Linq;
using System.Threading.Tasks;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using DataTable = Gherkin.Ast.DataTable;

namespace Nhs.Appointments.Api.Integration.Scenarios.Eula;

public abstract class BaseEulaFeatureSteps : BaseFeatureSteps
{
    [Given(@"the latest EULA is as follows")]
    public async Task SeedLatestEula(DataTable dataTable)
    {
        var cells = dataTable.Rows.Skip(1).Single().Cells;

        var versionDate = NaturalLanguageDate.Parse(cells.ElementAt(0).Value);
        var eulaVersion = new EulaDocument()
        {
            Id = "eula",
            DocumentType = "system",
            VersionDate = versionDate
        };

        await CosmosWrite(CosmosWriteAction.Upsert, "core_data", eulaVersion);
    }
}
