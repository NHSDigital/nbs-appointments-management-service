using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(string userId, IEnumerable<string> siteIds, string requiredPermission);
    Task<IEnumerable<string>> GetPermissionsAsync(string userId, string siteId);
}
