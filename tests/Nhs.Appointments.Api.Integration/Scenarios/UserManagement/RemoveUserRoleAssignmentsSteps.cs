using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Xunit.Gherkin.Quick;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement;

[FeatureFile("./Scenarios/UserManagement/RemoveUserRoleAssignments.feature")]
public sealed class RemoveUserRoleAssignmentsSteps : UserManagementBaseFeatureSteps
{
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private RemoveUserResponse _actualResponse;
    
    [When(@"I remove user '(.+)' from site '(.+)'")]
    public async Task AssignRole(string user, string site)
    {
        // Check user exists before removing them for assurance this method
        // actually works when it asserts that the removal has succeeded
        var document = await GetUserDocument(Client, user);
        document.Should().NotBeNull();

        var userId = GetUserId(user);
        var siteId = GetSiteId(site);

        var requestBody = new RemoveUserRequest()
        {
            User = userId,
            Site = siteId
        };

        _response = await Http.PostAsync(
            $"http://localhost:7071/api/user/remove", 
            new StringContent(
                JsonResponseWriter.Serialize(requestBody), 
                Encoding.UTF8, 
                "application/json"
            )
        );
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<RemoveUserResponse>(await _response.Content.ReadAsStreamAsync());
    }

    [Then(@"'(.+)' is no longer in the system")]
    public async Task Assert(string user)
    {
        var document = await GetUserDocument(Client, user);

        document.Should().BeNull();
    }

    [Then(@"user '(.+)' would have the following role assignments")]
    public async Task AssertRoleAssignments(string user, Gherkin.Ast.DataTable dataTable)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userId = GetUserId(user);
        var expectedRoleAssignments = dataTable.Rows.Skip(1).Select(row => new RoleAssignment
        {
            Scope = $"site:{GetSiteId(row.Cells.ElementAt(0).Value)}",
            Role = row.Cells.ElementAt(1).Value
        }).ToList();

        var actualResult = await Client
            .GetContainer("appts", "core_data")
            .ReadItemAsync<UserDocument>(userId, new PartitionKey("user"));

        actualResult.Resource.RoleAssignments.Should().BeEquivalentTo(expectedRoleAssignments);
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
        var actualResult = await Client.GetContainer("appts", "core_data").ReadItemAsync<UserDocument>(userId, new PartitionKey("user"));
        actualResult.Resource.RoleAssignments.Should().BeEquivalentTo(expectedRoleAssignments);
    }


    private async Task<UserDocument> GetUserDocument(CosmosClient cosmosClient, string user)
    {
        var container = cosmosClient.GetContainer("appts", "core_data");

        using(ResponseMessage response = await container.ReadItemStreamAsync(GetUserId(user), new PartitionKey("user")))
        {
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using (StreamReader streamReader = new StreamReader(response.Content))
            {
                string content = await streamReader.ReadToEndAsync();
                var userDocument = JsonConvert.DeserializeObject<UserDocument>(content);
                return userDocument;
            }
        }
    }
}
