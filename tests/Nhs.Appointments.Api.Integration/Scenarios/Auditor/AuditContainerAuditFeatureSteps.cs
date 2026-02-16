using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Persistance.Models;
using Notify.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Auditor;

[FeatureFile("./Scenarios/Auditor/AuditContainerAudit.feature")]
public sealed class AuditContainerAuditFeatureSteps : BaseFeatureSteps
{
    private readonly AuditHelper _auditHelper;
    private const string containerName = "audit_data";
    private HttpResponseMessage Response;

    public AuditContainerAuditFeatureSteps()
    {
        _auditHelper = new AuditHelper();
    }

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
        _response.EnsureSuccessStatusCode();
    }

    [Then(@"UserRolesChanged audit notification should match the notification in Cosmos DB")]
    public async Task VerifyNotificationAuditLog()
    {
        var (cosmosDoc, timeStamp) = await GetCosmosNotification();
        var fileName = _auditHelper.GetBlobName(
            cosmosDoc.DocumentType,
            timeStamp,
            cosmosDoc.Id
        );
        var auditJson = await _auditHelper.PollForAuditLogAsync(
            containerName,
            fileName
        );

        auditJson.Should().NotBeNullOrEmpty($"The audit log was not found or was empty.");

        var auditDoc = JsonConvert.DeserializeObject<AuditNotificationDocument>(auditJson);

        auditDoc.Should().BeEquivalentTo(cosmosDoc, options => options
            .Excluding(ctx => ctx.Path.Contains("_"))
        );
    }

    private async Task<(AuditNotificationDocument Document, DateTimeOffset Timestamp)> GetCosmosNotification()
    {
        var container = Client.GetContainer("appts", containerName);

        var queryDefinition = new QueryDefinition(
        @"SELECT * FROM c 
          WHERE c.destinationId = @email 
          AND c.notificationName = 'UserRolesChanged' 
          ORDER BY c.timestamp DESC")
        .WithParameter("@email", $"test-new-audit-user@nhs.net_{_testId}@nhs.net");

        using var iterator = container.GetItemQueryIterator<CosmosDocumentTestWrapper<AuditNotificationDocument>>(queryDefinition);

        var response = await iterator.ReadNextAsync();


        var wrapper = response.Resource.FirstOrDefault();

        return (wrapper.Document, wrapper.Timestamp);
    }
}
