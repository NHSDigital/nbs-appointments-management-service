using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Audit.Persistance;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public abstract class AuditFeatureSteps : BaseFeatureSteps
{
    [Then(@"an audit function document was created for user '(.+)' and function '(.+)'")]
    [And(@"an audit function document was created for user '(.+)' and function '(.+)'")]
    public async Task AssertAuditFunctionDocumentCreated(string user, string function)
    {
        await AssertAuditFunctionDocumentExists(user, function);
    }

    private async Task AssertAuditFunctionDocumentExists(string user, string functionName)
    {
        var timestamp = DateTime.UtcNow;
        var siteId = GetSiteId();
        var container = Client.GetContainer("appts", "audit_data");

        var auditFunctionDocument = await FindItemWithRetryAsync(container, user, functionName, siteId,
            TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));

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
                return documentsFound.Single(); //only one item should be found
            }

            await Task.Delay(delay); // Wait before retrying
        }

        throw new TimeoutException("AuditFunctionDocument not found within the timeout period.");
    }
}
