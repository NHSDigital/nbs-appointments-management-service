using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Persistance.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;
using Location = Nhs.Appointments.Core.Sites.Location;
using Site = Nhs.Appointments.Core.Sites.Site;

namespace Nhs.Appointments.Api.Integration.Scenarios.Auditor;

[FeatureFile("./Scenarios/Auditor/CoreContainerAudit.feature")]
public sealed class CoreContainerAuditFeatureSteps : BaseFeatureSteps
{
    private readonly AuditHelper _auditHelper;
    private const string containerName = "core_data";
    protected HttpResponseMessage Response { get; set; }
    public CoreContainerAuditFeatureSteps()
    {
        _auditHelper = new AuditHelper();
    }

    [Given(@"there are no role assignments for user '.+'")]
    public Task NoRoleAssignments() => Task.CompletedTask;

    [Given(@"user '(.+)' does not exist in the system")]
    public Task NoUser() => Task.CompletedTask;

    [When(@"I assign the following roles to user '(.+)'")]
    public async Task AssignRole(string user, DataTable dataTable)
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

        _response = await GetHttpClientForTest().PostAsJsonAsync($"http://localhost:7071/api/user/roles", payload);
    }

    [Then(@"user '(.+)' would have the following role assignments")]
    public async Task Assert(string user, DataTable dataTable)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userId = GetUserId(user);
        var actualResult = await Client.GetContainer("appts", "core_data").ReadItemAsync<UserDocument>(userId, new PartitionKey("user"));

        var expectedRoleAssignments = dataTable.Rows.Skip(1).Select(
            (row) => new RoleAssignment
            {
                Scope = $"site:{GetSiteId(row.Cells.ElementAt(0).Value)}",
                Role = row.Cells.ElementAt(1).Value
            });
        actualResult.Resource.RoleAssignments.Should().BeEquivalentTo(expectedRoleAssignments);
    }

    [And(@"an audit log should be created in StorageAccount for user '(.+)'")]
    public async Task VerifyAuditLogExists(string userEmail)
    {
        var (cosmosUser, timeStamp) = await GetCosmosUser(userEmail);
        var fileName = _auditHelper.GetBlobName(
            cosmosUser.DocumentType, 
            timeStamp, 
            cosmosUser.Id + "-" + cosmosUser.DocumentType
        );

        var content = await _auditHelper.PollForAuditLogAsync(
            containerName,
            fileName
        );

        content.Should().NotBeNull($"Audit log for {userEmail} was not found in StorageAccount within the timeout period.");
    }


    [And(@"the audit log for '(.+)' should match the Cosmos DB record")]
    public async Task VerifyAuditLogMatchesCosmos(string userEmail)
    {
        await VerifyAuditLogMatchesCosmosInternal(
            () => GetCosmosUser(userEmail),
            userEmail
        );
    }

    [When("I update the reference details for site '(.+)'")]
    public async Task UpdateSiteReferenceDetails(string siteDesignation, DataTable dataTable)
    {
        var siteId = GetSiteId(siteDesignation);
        var row = dataTable.Rows.ElementAt(1);

        var odsCode = row.Cells.ElementAt(0).Value;
        var icb = row.Cells.ElementAt(1).Value;
        var region = row.Cells.ElementAt(2).Value;

        var payload = new SetSiteReferenceDetailsRequest(siteId, odsCode, icb, region);
        Response = await GetHttpClientForTest().PostAsJsonAsync($"http://localhost:7071/api/sites/{siteId}/reference-details", payload);
    }

    [Then("the correct information for site '(.+)' is returned")]
    public async Task AssertSiteInformation(DataTable dataTable)
    {
        var row = dataTable.Rows.ElementAt(1);
        var siteId = row.Cells.ElementAt(0).Value;
        var expectedSite = new Site(
            Id: GetSiteId(siteId),
            Name: row.Cells.ElementAt(1).Value,
            Address: row.Cells.ElementAt(2).Value,
            PhoneNumber: row.Cells.ElementAt(3).Value,
            OdsCode: row.Cells.ElementAt(4).Value,
            Region: row.Cells.ElementAt(5).Value,
            IntegratedCareBoard: row.Cells.ElementAt(6).Value,
            InformationForCitizens: row.Cells.ElementAt(7).Value,
            Accessibilities: ParseAccessibilities(row.Cells.ElementAt(8).Value),
            new Location(
                Type: "Point",
                Coordinates: [double.Parse(row.Cells.ElementAt(9).Value), double.Parse(row.Cells.ElementAt(10).Value)]),
            status: null,
            isDeleted: dataTable.GetBoolRowValueOrDefault(row, "IsDeleted"),
            Type: dataTable.GetRowValueOrDefault(row, "Type")
        );
        Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var actualResult = await Client.GetContainer("appts", "core_data")
            .ReadItemAsync<Core.Sites.Site>(GetSiteId(siteId), new PartitionKey("site"));
        actualResult.Resource.Should().BeEquivalentTo(expectedSite);
    }

    [And(@"an audit log should be created in StorageAccount for site '(.+)'")]
    public async Task VerifyAuditLogExistsForSite(string siteId)
    {
        var (cosmosSite, timeStamp) = await GetCosmosSite(siteId);
        var fileName = _auditHelper.GetBlobName(
            cosmosSite.DocumentType, 
            timeStamp,
            cosmosSite.Id + "-" + cosmosSite.DocumentType
        );

        var content = await _auditHelper.PollForAuditLogAsync(
            containerName,
            fileName
        );

        content.Should().NotBeNull($"Audit log for site {siteId} was not found in StorageAccount within the timeout period.");
    }

    [And(@"the audit log for site '(.+)' should match the Cosmos DB record")]
    public async Task VerifySiteAuditLogMatchesCosmos(string site)
    {
        await VerifyAuditLogMatchesCosmosInternal(
            () => GetCosmosSite(site),
            site
        );
    }

    private async Task VerifyAuditLogMatchesCosmosInternal<TDocument>(
        Func<Task<(TDocument Document, DateTimeOffset Timestamp)>> getCosmosData,
        string identifier) where TDocument : TypedCosmosDocument
    {
        var (cosmosDoc, timeStamp) = await getCosmosData();
        var fileName = _auditHelper.GetBlobName(
            cosmosDoc.DocumentType, 
            timeStamp,
            cosmosDoc.Id + "-" + cosmosDoc.DocumentType
        );
        var auditJson = await _auditHelper.PollForAuditLogAsync(
            containerName,
            fileName
        );

        auditJson.Should().NotBeNullOrEmpty($"The audit log for {identifier} was not found or was empty.");

        var auditDoc = JsonConvert.DeserializeObject<TDocument>(auditJson);

        auditDoc.Should().BeEquivalentTo(cosmosDoc, options => options
            .Excluding(ctx => ctx.Path.Contains("_"))
        );
    }

    private async Task<(UserDocument Document, DateTimeOffset Timestamp)> GetCosmosUser(string userEmail)
    {
        var userId = GetUserId(userEmail);
        var container = Client.GetContainer("appts", containerName);

        var response = await container.ReadItemAsync<CosmosDocumentTestWrapper<UserDocument>>(
            userId,
            new PartitionKey("user")
        );

        var wrapper = response.Resource;

        return (wrapper.Document, wrapper.Timestamp);
    }

    private async Task<(SiteDocument Document, DateTimeOffset Timestamp)> GetCosmosSite(string site)
    {
        var siteId = GetSiteId(site);
        var container = Client.GetContainer("appts", containerName);

        var response = await container.ReadItemAsync<CosmosDocumentTestWrapper<SiteDocument>>(
            siteId,
            new PartitionKey("site")
        );

        var wrapper = response.Resource;

        return (wrapper.Document, wrapper.Timestamp);
    }
}
