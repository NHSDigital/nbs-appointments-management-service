using System.Linq;
using System.Threading.Tasks;
using Gherkin.Ast;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement;

public abstract class UserManagementBaseFeatureSteps : BaseFeatureSteps
{
    [Given(@"there are no role assignments for user '.+'")]
    public Task NoRoleAssignments() => Task.CompletedTask;

    [Given(@"the following role assignments at the default site for '(.+)' exist")]
    [And(@"the following role assignments at the default site for '(.+)' exist")]
    public async Task AddRoleAssignmentsDefaultSite(string user, DataTable dataTable)
    {
        var roleAssignments = dataTable.Rows.Skip(1).Select(
            row => new RoleAssignment
            {
                Scope = $"site:{GetSiteId()}",
                Role = row.Cells.ElementAt(0).Value
            }).ToArray();
        var userDocument = new UserDocument()
        {
            Id = GetUserId(user),
            DocumentType = "user",
            RoleAssignments = roleAssignments
        };
        await CosmosWrite(CosmosWriteAction.Create, "core_data", userDocument);
    }
    
    [Given(@"the following role assignments at site '(.+)' for '(.+)' exist")]
    [And(@"the following role assignments at site '(.+)' for '(.+)' exist")]
    public async Task AddRoleAssignmentsForSite(string site, string user, DataTable dataTable)
    {
        var roleAssignments = dataTable.Rows.Skip(1).Select(
            row => new RoleAssignment
            {
                Scope = $"site:{GetSiteId(site)}",
                Role = row.Cells.ElementAt(0).Value
            }).ToArray();
        var userDocument = new UserDocument()
        {
            Id = GetUserId(user),
            DocumentType = "user",
            RoleAssignments = roleAssignments
        };
        await CosmosWrite(CosmosWriteAction.Create, "core_data", userDocument);
    }
    
    [Given(@"the following role assignments for '(.+)' exist")]
    [And(@"the following role assignments for '(.+)' exist")]
    public async Task AddRoleAssignments(string user, DataTable dataTable)
    {
        var roleAssignments = dataTable.Rows.Skip(1).Select(
            row => new RoleAssignment
            {
                Scope = $"site:{GetSiteId(row.Cells.ElementAt(0).Value)}",
                Role = row.Cells.ElementAt(1).Value
            }).ToArray();
        var userDocument = new UserDocument()
        {
            Id = GetUserId(user),
            DocumentType = "user",
            RoleAssignments = roleAssignments
        };
        await CosmosWrite(CosmosWriteAction.Create, "core_data", userDocument);
    }
}
