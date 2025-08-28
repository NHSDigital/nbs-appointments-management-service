using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using RoleAssignment = Nhs.Appointments.Persistance.Models.RoleAssignment;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement.ProposeCreation;

public abstract class ProposeCreationBaseFeatureSteps(string flag, bool enabled) : UserManagementBaseFeatureSteps(flag, enabled)
{
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private ProposePotentialUserResponse _actualResponse;
    
    protected RoleAssignment SomeRoleAssignment() =>
        new() { Scope = $"site:{GetSiteId()}", Role = "canned:site-details-manager" };

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
