using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using DataTable = Gherkin.Ast.DataTable;

namespace Nhs.Appointments.Api.Integration.Scenarios.Eula;

[FeatureFile("./Scenarios/Eula/GetEula.feature")]
public sealed class GetEulaFeatureSteps : BaseEulaFeatureSteps
{
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private EulaVersion _actualResponse;

    [And(@"I request the latest EULA version")]
    public async Task RequestLatestEula()
    {
        _response = await Http.GetAsync($"http://localhost:7071/api/eula");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<EulaVersion>(await _response.Content.ReadAsStreamAsync());
    }

    [Then(@"the following EULA is returned")]
    public void Assert(DataTable dataTable)
    {
        var cells = dataTable.Rows.Skip(1).Single().Cells;

        var versionDate = ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value);
        var eulaVersion = new EulaVersion()
        {
            VersionDate = versionDate
        };

        _statusCode.Should().Be(System.Net.HttpStatusCode.OK);
        _actualResponse.Should().BeEquivalentTo(eulaVersion);
    }
}