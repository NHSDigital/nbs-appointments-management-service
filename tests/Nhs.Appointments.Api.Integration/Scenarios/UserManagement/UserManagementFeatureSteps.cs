using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement;

[FeatureFile("./Scenarios/UserManagement/UserManagement.feature")]
public sealed class UserManagementFeatureSteps : BaseFeatureSteps
{
    private  HttpResponseMessage _response;
    
    [Given(@"There are no role assignments for user '.+'")]
    public Task NoRoleAssignments()
    {
        return Task.CompletedTask;
    }

    [Given(@"The following role assignments for '(.+)' exist")]
    public async Task AddRoleAssignments(string user, Gherkin.Ast.DataTable dataTable)
    {
        var roleAssignments = dataTable.Rows.Skip(1).Select(
            row => new RoleAssignment()
            {
                Scope = row.Cells.ElementAt(0).Value,
                Role = row.Cells.ElementAt(1).Value
            }).ToArray();
        var userDocument = new UserDocument()
            {
                Id = GetUserId(user),
                DocumentType = "user",
                RoleAssignments = roleAssignments
            };    
        await Client.GetContainer("appts", "index_data").CreateItemAsync(userDocument);
    }

    [When(@"I assign the following roles to user '(.+)'")]
    public async Task AssignRole(string user, Gherkin.Ast.DataTable dataTable)
    {
        if (dataTable.Rows.Count() > 2)
            throw new InvalidOperationException("This step only allows one row of data");

        var row = dataTable.Rows.ElementAt(1);
        
        var payload = new
        {
            user = GetUserId(user),
            scope = row.Cells.ElementAt(0).Value,
            roles = row.Cells.ElementAt(1).Value.Split(",")
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
                Scope = row.Cells.ElementAt(0).Value,
                Role = row.Cells.ElementAt(1).Value
            });
        var actualResult = await Client.GetContainer("appts", "index_data").ReadItemAsync<UserDocument>(userId, new Microsoft.Azure.Cosmos.PartitionKey("user"));
        actualResult.Resource.RoleAssignments.Should().BeEquivalentTo(expectedRoleAssignments);
    }
}
