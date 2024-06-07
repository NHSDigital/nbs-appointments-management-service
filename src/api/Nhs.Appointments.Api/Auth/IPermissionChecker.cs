using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(string userId, string requiredPermission);
}
