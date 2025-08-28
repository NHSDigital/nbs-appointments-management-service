using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.RoleManagement;

[FeatureFile("./Scenarios/RoleManagement/RoleManagement.feature")]
public sealed class RoleManagementFeatureSteps : BaseFeatureSteps
{
    private  HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private GetRolesResponse _actualResponse;
    
    [Given("There are existing roles")]
    public Task Roles()
    {
        // Roles already setup by SetUpRoles() in BaseFeatureSteps
        return Task.CompletedTask;
    }
    
    [When(@"I query for all '([\w]+)' roles")]
    public async Task QueryForRoles(string tag)
    {
        _response = await Http.GetAsync($"http://localhost:7071/api/roles?tag={tag}");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<GetRolesResponse>(await _response.Content.ReadAsStreamAsync());
    }

    [Then("The following roles are returned")] 
    public void Assert(Gherkin.Ast.DataTable dataTable)
    {
        _statusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var expectedRoleItems = dataTable.Rows.Skip(1)
            .Select((row) => 
                new GetRoleResponseItem(row.Cells.ElementAt(0).Value, row.Cells.ElementAt(1).Value, row.Cells.ElementAt(2).Value));
        var expectedRolesResponse = new GetRolesResponse(expectedRoleItems);
        _actualResponse.Should().BeEquivalentTo(expectedRolesResponse);
    }
}
