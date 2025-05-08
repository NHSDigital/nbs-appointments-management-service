using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement.ProposeCreation;

public abstract class ProposeCreationBaseFeatureSteps : UserManagementBaseFeatureSteps
{
    private RoleAssignment SomeRoleAssignment() =>
        new() { Scope = $"site:{GetSiteId()}", Role = "canned:site-details-manager" };

    [Given(@"user '(.+)' exists in MYA")]
    [And(@"user '(.+)' exists in MYA")]
    public async Task EnsureUserExists(string userEmail)
    {
        var userDocument = new UserDocument
        {
            Id = userEmail, DocumentType = "user", RoleAssignments = [SomeRoleAssignment()]
        };

        await Client.GetContainer("appts", "core_data").CreateItemAsync(userDocument);
    }

    [Given(@"user '(.+)' does not exist in MYA")]
    [And(@"user '(.+)' does not exist in MYA")]
    public async Task EnsureUserDoesNotExist(string userEmail)
    {
        try
        {
            await Client.GetContainer("appts", "core_data")
                .DeleteItemAsync<UserDocument>(userEmail, new PartitionKey("user"));
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Cosmos does not support checking ahead for null from ReadItemAsync, so
            // using exceptions for control flow here is a necessary evil
            // https://github.com/Azure/azure-cosmos-dotnet-v3/issues/692
        }
    }
}
