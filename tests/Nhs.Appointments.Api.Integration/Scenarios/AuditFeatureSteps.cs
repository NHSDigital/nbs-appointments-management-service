using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Audit.Persistance;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public abstract class AuditFeatureSteps : BaseFeatureSteps
{
    [Then(@"an audit function document was created for user '(.+)' and function '(.+)'")]
    [And(@"an audit function document was created for user '(.+)' and function '(.+)'")]
    public async Task AssertAuditFunctionDocumentCreated(string user, string function) =>
        await AssertAuditFunctionDocumentExists(user, function, GetSiteId());

    [Then(@"an audit function document was created for")]
    [And(@"an audit function document was created for")]
    public async Task AssertAuditFunctionDocumentFor(DataTable table)
    {
        var row = table.Rows.ElementAt(1);

        var user = table.GetRowValueOrDefault(row, "User");
        var functionName = table.GetRowValueOrDefault(row, "Function Name");
        var site = GetSiteId(table.GetRowValueOrDefault(row, "Site"));

        await AssertAuditFunctionDocumentExists(user, functionName, site);
    }

    [Then(@"an audit function document was created for user '(.+)' and function '(.+)' and no site")]
    [And(@"an audit function document was created for user '(.+)' and function '(.+)' and no site")]
    public async Task AssertAuditFunctionDocumentCreatedWithNoSite(string user, string function) =>
        await AssertAuditFunctionDocumentExists(user, function, null);

    private async Task AssertAuditFunctionDocumentExists(string user, string functionName, string siteId)
    {
        var timestamp = DateTime.UtcNow;
        var container = Client.GetContainer("appts", "audit_data");

        var auditFunctionDocument = await FindItemWithRetryAsync(container, user, functionName, siteId,
            TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(1));

        //confirm that the document was created recently, as cannot easily verify via id query
        auditFunctionDocument.Timestamp.Should().BeBefore(timestamp);
        auditFunctionDocument.Timestamp.Should().BeOnOrAfter(timestamp.AddSeconds(-10));
    }

    private static async Task<AuditFunctionDocument> FindItemWithRetryAsync(Container container, string user,
        string functionName, string siteId, TimeSpan timeout, TimeSpan delay)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            var documentsFound = (await RunQueryAsync<AuditFunctionDocument>(container,
                d => d.User == user && d.FunctionName == functionName && d.Site == siteId)).ToList();

            if (documentsFound.Count > 0)
            {
                // Audit logs with no site will not be unique on test reruns, so there may be duplicate on the 2nd run
                if (siteId is not null)
                {
                    return documentsFound.Single();
                }

                return documentsFound.OrderByDescending(doc => doc.Timestamp).First();
            }

            await Task.Delay(delay); // Wait before retrying
        }

        throw new TimeoutException("AuditFunctionDocument not found within the timeout period.");
    }
}
