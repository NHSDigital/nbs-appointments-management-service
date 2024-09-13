using System.Linq;
using System.Threading.Tasks;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement;

public abstract class UserManagementBaseFeatureSteps : BaseFeatureSteps
{
    [Given(@"The following role assignments for '(.+)' exist")]
    [And(@"the following role assignments for '(.+)' exist")]
    public async Task AddRoleAssignments(string user, Gherkin.Ast.DataTable dataTable)
    {
        SetUpUserRoleAssignments(user);
        var roleAssignments = dataTable.Rows.Skip(1).Select(
            row => new RoleAssignment()
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
        await Client.GetContainer("appts", "index_data").CreateItemAsync(userDocument);
    }
}
