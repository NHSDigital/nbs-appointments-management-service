using Nhs.Appointments.Core;
using Okta.Sdk.Model;

namespace Nhs.Appointments.UserManagement.Okta;

public class OktaService(IOktaUserDirectory oktaUserService) : IOktaService
{
    public async Task<UserProvisioningStatus> CreateIfNotExists(string userEmail, string? firstName, string? lastName)
    {
        var success = false;
        var failureReason = string.Empty;
        var oktaUser = await oktaUserService.GetUserAsync(userEmail);

        if (oktaUser != null)
        {
            var isOlderThanOneDay = DateTime.UtcNow - oktaUser.Created > TimeSpan.FromDays(1);

            if (oktaUser.Status == UserStatus.PROVISIONED && isOlderThanOneDay)
            {
                success = await oktaUserService.ReactivateUserAsync(userEmail); ;
            }
            else if (oktaUser.Status == UserStatus.ACTIVE)
            {
                success = true;
            }
        }
        else
        {
            success = await oktaUserService.CreateUserAsync(userEmail, firstName, lastName);
            if (!success)
            {
                failureReason = "User could not be created";
            }
        }

        return new UserProvisioningStatus{ Success = success, FailureReason = failureReason };
    }
}
