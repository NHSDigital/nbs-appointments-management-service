using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement;

[FeatureFile("./Scenarios/UserManagement/GetUserRoleAssignments.feature")]
public sealed class GetUserRoleAssignmentsSteps : UserManagementBaseFeatureSteps
{
    private  HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private IEnumerable<User> _actualResponse;
    
    [When(@"I request all user role assignments for site '(\w)'")]
    public async Task AssignRole(string site)
    {
        var siteId = GetSiteId(site);
        _response = await Http.GetAsync($"http://localhost:7071/api/users?site={siteId}");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<IEnumerable<User>>(await _response.Content.ReadAsStreamAsync());
    }
    
    [Then(@"the following list of user role assignments is returned")] 
    public async Task Assert(Gherkin.Ast.DataTable dataTable)
    {
        var expectedUserRoleAssignments = dataTable.Rows.Skip(1).Select(
            row => new User
            {
                Id = GetUserId(row.Cells.ElementAt(0).Value), 
                RoleAssignments = [new RoleAssignment 
                    { Scope = $"site:{GetSiteId(row.Cells.ElementAt(1).Value)}", Role = row.Cells.ElementAt(2).Value }]
            });
        _statusCode.Should().Be(System.Net.HttpStatusCode.OK);
        _actualResponse.Should().BeEquivalentTo(expectedUserRoleAssignments);
    }
}
