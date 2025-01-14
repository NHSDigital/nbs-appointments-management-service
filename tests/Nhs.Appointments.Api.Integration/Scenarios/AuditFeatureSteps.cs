using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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
        var auditFunctionDocuments = await RunQueryAsync<AuditFunctionDocument>(container,
            d => d.User == user && d.FunctionName == functionName && d.Site == siteId);

        var latestDocument = auditFunctionDocuments.OrderByDescending(d => d.Timestamp).Single();

        //confirm that the document was created recently, as cannot easily verify via id query
        latestDocument.Timestamp.Should().BeBefore(timestamp);
        latestDocument.Timestamp.Should().BeOnOrAfter(timestamp.AddSeconds(-30));
    }
}
