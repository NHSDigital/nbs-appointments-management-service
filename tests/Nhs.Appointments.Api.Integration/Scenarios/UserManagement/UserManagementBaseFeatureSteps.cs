using System.Linq;
using System.Threading.Tasks;
using Gherkin.Ast;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UserManagement;

public abstract class UserManagementBaseFeatureSteps : BaseFeatureSteps
{
    [Given(@"There are no role assignments for user '.+'")]
    public Task NoRoleAssignments() => Task.CompletedTask;
}
