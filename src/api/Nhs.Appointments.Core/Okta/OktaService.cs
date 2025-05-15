namespace Nhs.Appointments.Core.Okta;

public class OktaService(IOktaUserDirectory oktaUserDirectory, TimeProvider timeProvider) : IOktaService
{
    public async Task<UserProvisioningStatus> CreateIfNotExists(string userEmail, string firstName, string lastName)
    {
        var userState = await GetUserState(userEmail);
        return userState switch
        {
            UserState.UserDoesNotExist => await CreateUser(userEmail, firstName, lastName),
            UserState.UserWasProvisionedButOver24HoursAgo => await ReactivateUser(userEmail),
            UserState.UserWasProvisionedButUnder24HoursAgo or UserState.UserIsActive => new UserProvisioningStatus
            {
                Success = true
            },
            _ => new UserProvisioningStatus
            {
                Success = false, FailureReason = "Failed to identify if user was active, provisioned or not extant."
            }
        };
    }

    private async Task<UserProvisioningStatus> CreateUser(string email, string firstName, string lastName)
    {
        var createdSuccessfully = await oktaUserDirectory.CreateUserAsync(email, firstName, lastName);
        return new UserProvisioningStatus
        {
            Success = createdSuccessfully,
            FailureReason = !createdSuccessfully ? "Failed to create the user" : string.Empty
        };
    }

    private async Task<UserProvisioningStatus> ReactivateUser(string email)
    {
        var reactivatedSuccessfully = await oktaUserDirectory.ReactivateUserAsync(email);
        return new UserProvisioningStatus
        {
            Success = reactivatedSuccessfully,
            FailureReason = !reactivatedSuccessfully ? "Failed to reactivate the user" : string.Empty
        };
    }

    private enum UserState
    {
        Unknown,
        UserDoesNotExist,
        UserWasProvisionedButOver24HoursAgo,
        UserWasProvisionedButUnder24HoursAgo,
        UserIsActive
    }

    private async Task<UserState> GetUserState(string userEmail)
    {
        var user = await oktaUserDirectory.GetUserAsync(userEmail);
        if (user is null)
        {
            return UserState.UserDoesNotExist;
        }

        var isOlderThanOneDay = timeProvider.GetUtcNow() - user.Created > TimeSpan.FromDays(1);
        if (user.IsProvisioned && isOlderThanOneDay)
        {
            return UserState.UserWasProvisionedButOver24HoursAgo;
        }

        if (user.IsProvisioned && !isOlderThanOneDay)
        {
            return UserState.UserWasProvisionedButUnder24HoursAgo;
        }

        return user.IsActive ? UserState.UserIsActive : UserState.Unknown;
    }
}
