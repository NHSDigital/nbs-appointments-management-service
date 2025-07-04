using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement;

[FeatureFile("./Scenarios/UserManagement/AssignRolesToAUser.feature")]
public sealed class AssignRolesToAUserFeatureSteps() : UserManagementBaseFeatureSteps(Flags.OktaEnabled, false)
{
    private  HttpResponseMessage _response;

    [When(@"I assign the following roles to user '(.+)'")]
    public async Task AssignRole(string user, Gherkin.Ast.DataTable dataTable)
    {
        if (dataTable.Rows.Count() > 2)
            throw new InvalidOperationException("This step only allows one row of data");

        var row = dataTable.Rows.ElementAt(1);
        
        var payload = new
        {
            user = GetUserId(user),
            scope = $"site:{GetSiteId(row.Cells.ElementAt(0).Value)}",
            roles = row.Cells.ElementAt(1).Value.Split(","),
            firstName = "firstName",
            lastName = "lastName"
        };
        
        _response = await Http.PostAsJsonAsync($"http://localhost:7071/api/user/roles", payload);
    }

    [Then(@"user '(.+)' would have the following role assignments")] 
    public async Task Assert(string user, Gherkin.Ast.DataTable dataTable)
    {
        _response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var userId = GetUserId(user);
        var expectedRoleAssignments = dataTable.Rows.Skip(1).Select(
            (row) => new RoleAssignment
            {
                Scope = $"site:{GetSiteId(row.Cells.ElementAt(0).Value)}",
                Role = row.Cells.ElementAt(1).Value
            });
        var actualResult = await Client.GetContainer("appts", "core_data").ReadItemAsync<UserDocument>(userId, new Microsoft.Azure.Cosmos.PartitionKey("user"));
        actualResult.Resource.RoleAssignments.Should().BeEquivalentTo(expectedRoleAssignments);
    }
}
