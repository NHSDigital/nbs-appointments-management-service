using Gherkin.Ast;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.CancelSession;

[FeatureFile("./Scenarios/CancelSession/CancelSession.feature")]
public class CancelSessionFeatureSteps : BaseFeatureSteps
{
    protected HttpResponseMessage Response { get; set; }

    [When("I cancel the following session")]
    public async Task CancelSession(DataTable dataTable)
    {
        var cells = dataTable.Rows.ElementAt(1).Cells;

        object payload = new
        {
            site = GetSiteId(),
            date = DateTime.ParseExact(ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value).ToString("yyyy-MM-dd"), "yyyy-MM-dd", null),
            from = cells.ElementAt(1).Value,
            until = cells.ElementAt(2).Value,
            services = new string[] { cells.ElementAt(3).Value },
            slotLength = cells.ElementAt(4).Value,
            capacity = cells.ElementAt(5).Value
        };

        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/session/cancel", payload);
    }
}
