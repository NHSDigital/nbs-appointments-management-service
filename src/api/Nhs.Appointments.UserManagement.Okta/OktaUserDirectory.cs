using Nhs.Appointments.Core;
using Okta.Sdk.Model;

namespace Nhs.Appointments.UserManagement.Okta;

public class OktaUserDirectory(IOktaUserService oktaUserService) : IUserDirectory
{
    public async Task<UserProvisioningStatus> CreateIfNotExists(string user)
    {
        var success = false;
        var failureReason = string.Empty;
        var userr = await oktaUserService.GetUserAsync(user);

        if (userr != null)
        {
            var isOlderThanOneDay = DateTime.UtcNow - userr.Created > TimeSpan.FromDays(1);

            if (userr.Status == UserStatus.PROVISIONED && isOlderThanOneDay)
            {
                success = await oktaUserService.ReactivateUserAsync(user); ;
            }
            else if (userr.Status == UserStatus.ACTIVE)
            {
                success = true;
            }
        }
        else
        {
            success = await oktaUserService.CreateUserAsync(user);
            if (!success)
            {
                failureReason = "User could not be created";
            }
        }

        return new UserProvisioningStatus{ Success = success, FailureReason = failureReason };
    }
}
