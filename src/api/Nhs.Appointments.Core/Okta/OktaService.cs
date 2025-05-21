namespace Nhs.Appointments.Core.Okta;

public class OktaService(IOktaUserDirectory oktaUserDirectory, TimeProvider timeProvider) : IOktaService
{
    public async Task<UserProvisioningStatus> CreateIfNotExists(string userEmail, string firstName, string lastName)
    {
        var oktaUser = await oktaUserDirectory.GetUserAsync(userEmail);

        switch (oktaUser?.Status)
        {
            case null:
                return await CreateUser(userEmail, firstName, lastName);
            case OktaUserStatus.Active:
                return new UserProvisioningStatus { Success = true };
            case OktaUserStatus.Provisioned:
                {
                    var isOlderThanOneDay = timeProvider.GetUtcNow() - oktaUser.Created > TimeSpan.FromDays(1);
                    if (isOlderThanOneDay)
                    {
                        return await ReactivateUser(userEmail);
                    }

                    return new UserProvisioningStatus { Success = true };
                }
            case OktaUserStatus.Deactivated:
                return new UserProvisioningStatus { Success = false, FailureReason = "User account is deactivated." };
            default:
                return new UserProvisioningStatus
                {
                    Success = false,
                    FailureReason = "Failed to identify if user was active, provisioned or not extant."
                };
        }
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
}
