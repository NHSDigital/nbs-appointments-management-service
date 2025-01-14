using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Gherkin.Ast;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.ApiClient.Auth;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;
using Feature = Xunit.Gherkin.Quick.Feature;
using Location = Nhs.Appointments.Core.Location;
using Role = Nhs.Appointments.Persistance.Models.Role;
using RoleAssignment = Nhs.Appointments.Persistance.Models.RoleAssignment;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public abstract partial class AuditFeatureSteps : BaseFeatureSteps
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
        var auditFunctionDocuments = await RunQueryAsync<AuditFunctionDocument>(container, d => d.User == user && d.FunctionName == functionName && d.Site == siteId);
        
        var latestDocument = auditFunctionDocuments.OrderByDescending(d => d.Timestamp).Single();
        
        //confirm that the document was created recently, as cannot easily verify via id query
        latestDocument.Timestamp.Should().BeBefore(timestamp);
        latestDocument.Timestamp.Should().BeOnOrAfter(timestamp.AddSeconds(30));
    }
}
