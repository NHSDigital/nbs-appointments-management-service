using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement.ProposeCreation;

[FeatureFile("./Scenarios/UserManagement/ProposeCreation/ProposePotentialNewUser.feature")]
public class ProposePotentialNewUserSteps : ProposeCreationBaseFeatureSteps
{
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private ProposePotentialUserResponse _actualResponse;

    [When(@"I propose creating user '(.+)'")]
    public async Task ProposeNewUser(string userEmail)
    {
        var requestBody = new ProposePotentialUserRequest(GetSiteId(), userEmail);

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

    [Then(@"the request should be successful")]
    public async Task AssertHttpOk() => _statusCode.Should().Be(HttpStatusCode.OK);

    [Then(@"the user's current status is returned as follows")]
    public async Task AssertRoleAssignments(DataTable dataTable)
    {
        var cells = dataTable.Rows.Skip(1).Single().Cells.ToList();

        var expectedResponse = new ProposePotentialUserResponse
        {
            ExtantInSite = bool.Parse(cells.ElementAt(0).Value),
            ExtantInIdentityProvider = bool.Parse(cells.ElementAt(1).Value),
            IdentityProvider = Enum.Parse<IdentityProvider>(cells.ElementAt(2).Value),
            MeetsWhitelistRequirements = bool.Parse(cells.ElementAt(3).Value)
        };

        _actualResponse.Should().BeEquivalentTo(expectedResponse);
    }
}
