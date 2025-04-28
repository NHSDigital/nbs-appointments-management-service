using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement.ProposeCreation;

[FeatureFile("./Scenarios/UserManagement/ProposeCreation/ProposePotentialNewUser.feature")]
public class ProposePotentialNewUserSteps : ProposeCreationBaseFeatureSteps
{
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private ProposePotentialUserResponse _actualResponse;

    [When(@"I propose creating user '(.+)'")]
    public async Task ProposeNewUser(string userId)
    {
        var requestBody = new ProposePotentialUserRequest(GetSiteId(), userId);

        _response = await Http.PostAsync(
            "http://localhost:7071/api/user/propose-potential",
            new StringContent(
                JsonResponseWriter.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            )
        );
        _statusCode = _response.StatusCode;
        (_, _actualResponse) =
            await JsonRequestReader.ReadRequestAsync<ProposePotentialUserResponse>(
                await _response.Content.ReadAsStreamAsync());
    }
}
