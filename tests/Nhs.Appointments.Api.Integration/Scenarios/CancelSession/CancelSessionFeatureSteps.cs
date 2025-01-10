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
        var date = DateTime.ParseExact(ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value).ToString("yyyy-MM-dd"), "yyyy-MM-dd", null);

        object payload = new
        {
            site = GetSiteId(),
            date = DateOnly.FromDateTime(date),
            until = cells.ElementAt(2).Value,
            from = cells.ElementAt(1).Value,
            services = new string[] { cells.ElementAt(3).Value },
            slotLength = int.Parse(cells.ElementAt(4).Value),
            capacity = int.Parse(cells.ElementAt(5).Value)
        };

        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/session/cancel", payload);
    }
}
