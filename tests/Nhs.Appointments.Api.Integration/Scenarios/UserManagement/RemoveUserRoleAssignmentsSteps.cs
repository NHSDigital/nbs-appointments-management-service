using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using FluentAssertions;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement;

[FeatureFile("./Scenarios/UserManagement/RemoveUserRoleAssignments.feature")]
public sealed class RemoveUserRoleAssignmentsSteps : UserManagementBaseFeatureSteps
{
    [When(@"I remove user '(.+)' from site '(.+)'")]
    public async Task AssignRole(string user, string site)
    {
        var userId = GetUserId(user);
        var siteId = GetSiteId(site);
        
        // Check user exists before removing them for assurance this method
        // actually works when it asserts that the removal has succeeded
        var document =
            await CosmosReadItem<UserDocument>("core_data", userId, new PartitionKey("user"), CancellationToken.None);
        
        document.Resource.Should().NotBeNull();
        
        var requestBody = new RemoveUserRequest()
        {
            User = userId,
            Site = siteId
        };

        _response = await GetHttpClientForTest().PostAsync(
            $"http://localhost:7071/api/user/remove", 
            new StringContent(
                JsonResponseWriter.Serialize(requestBody), 
                Encoding.UTF8, 
                "application/json"
            )
        );
    }

    [Then(@"'(.+)' is no longer in the system")]
    public async Task AssertUserDeleted(string user)
    {
        var userId = GetUserId(user);

        var exception = await Assert.ThrowsAsync<CosmosException>(async () =>
            await CosmosReadItem<UserDocument>("core_data", userId, new PartitionKey("user"), CancellationToken.None));
        
        exception.Message.Should().Contain("404");
    }

    [Then(@"user '(.+)' would have the following role assignments")]
    public async Task AssertRoleAssignments(string user, DataTable dataTable)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userId = GetUserId(user);
        var expectedRoleAssignments = dataTable.Rows.Skip(1).Select(row => new RoleAssignment
        {
            Scope = $"site:{GetSiteId(row.Cells.ElementAt(0).Value)}",
            Role = row.Cells.ElementAt(1).Value
        }).ToList();
        
        var document =
            await CosmosReadItem<UserDocument>("core_data", userId, new PartitionKey("user"), CancellationToken.None);

        document.Resource.RoleAssignments.Should().BeEquivalentTo(expectedRoleAssignments);
    }
}
