using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using DataTable = Gherkin.Ast.DataTable;

namespace Nhs.Appointments.Api.Integration.Scenarios.Eula;

[FeatureFile("./Scenarios/Eula/ConsentToEula.feature")]
public sealed class ConsentToEulaFeatureSteps : BaseEulaFeatureSteps
{
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private UserProfile _actualResponse;
    private HttpClient _eulaUserHttpClient; 

    [And(@"the current user has agreed the EULA on the following date")]
    public async Task UpsertIntTestUserEulaDate(DataTable dataTable)
    {
        var cells = dataTable.Rows.Skip(1).Single().Cells;
        var versionDate = ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value);

        await SetupEulaUser(versionDate);
    }

    [When(@"the current user agrees to a EULA with the following date")]
    public async Task ConsentToLatestEula(DataTable dataTable)
    {
        var cells = dataTable.Rows.Skip(1).Single().Cells;
        var versionDate = ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value);

        var payload = new
        {
            versionDate = $"{versionDate:yyyy-MM-dd}"
        };

        var requestSigner = new RequestSigningHandler(new RequestSigner(TimeProvider.System, ApiSigningKey));
        requestSigner.InnerHandler = new HttpClientHandler();
        _eulaUserHttpClient = new HttpClient(requestSigner);
        _eulaUserHttpClient.DefaultRequestHeaders.Add("ClientId", "eulaTestUser");
        _response = await _eulaUserHttpClient.PostAsync($"http://localhost:7071/api/eula/consent", new StringContent(JsonResponseWriter.Serialize(payload), Encoding.UTF8, "application/json"));
    }

    [Then(@"the current user now has the following latest agreed EULA date")]
    public async Task AssertLatestUserEulaVersion(DataTable dataTable)
    {
        var cells = dataTable.Rows.Skip(1).Single().Cells;
        var versionDate = ParseNaturalLanguageDateOnly(cells.ElementAt(0).Value);

        _response = await _eulaUserHttpClient.GetAsync($"http://localhost:7071/api/user/profile");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<UserProfile>(await _response.Content.ReadAsStreamAsync());

        _actualResponse.LatestAcceptedEulaVersion.Should().Be(versionDate);
    }

    private async Task SetupEulaUser(DateOnly latestAcceptedEulaVersion = default)
    {
        var userAssignments = new UserDocument()
        {
            
            Id = "api@eulaTestUser",
            ApiSigningKey = ApiSigningKey,
            DocumentType = "user",
            RoleAssignments = [
                new RoleAssignment()
                    { Role = "system:integration-test-user", Scope = "global" }
            ],
            LatestAcceptedEulaVersion = latestAcceptedEulaVersion
        };        
        await Client.GetContainer("appts", "core_data").UpsertItemAsync(userAssignments);
    }
}
