using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Integration.Data;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using DataTable = Gherkin.Ast.DataTable;

namespace Nhs.Appointments.Api.Integration.Scenarios.Eula;

[FeatureFile("./Scenarios/Eula/ConsentToEula.feature")]
public sealed class ConsentToEulaFeatureSteps : BaseEulaFeatureSteps
{
    private UserProfile _actualResponse;

    [When(@"the api user agrees to a EULA with the following date")]
    public async Task ConsentToLatestEula(DataTable dataTable)
    {
        var cells = dataTable.Rows.Skip(1).Single().Cells;
        var versionDate = NaturalLanguageDate.Parse(cells.ElementAt(0).Value);

        var payload = new
        {
            versionDate = $"{versionDate:yyyy-MM-dd}"
        };
       
        _response = await GetHttpClientForTest().PostAsync($"http://localhost:7071/api/eula/consent", new StringContent(JsonResponseWriter.Serialize(payload), Encoding.UTF8, "application/json"));
    }

    [Then(@"the api user now has the following latest agreed EULA date")]
    public async Task AssertLatestUserEulaVersion(DataTable dataTable)
    {
        var cells = dataTable.Rows.Skip(1).Single().Cells;
        var versionDate = NaturalLanguageDate.Parse(cells.ElementAt(0).Value);

        _response = await GetHttpClientForTest().GetAsync($"http://localhost:7071/api/user/profile");
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<UserProfile>(await _response.Content.ReadAsStreamAsync());

        _actualResponse.LatestAcceptedEulaVersion.Should().Be(versionDate);
    }
}
