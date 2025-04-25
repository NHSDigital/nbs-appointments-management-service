namespace Nhs.Appointments.Core.Okta;

public class OktaService(IOktaUserDirectory oktaUserDirectory) : IOktaService
{
    public async Task<UserProvisioningStatus> CreateIfNotExists(string userEmail, string? firstName, string? lastName)
    {
        var success = false;
        var oktaUser = await oktaUserDirectory.GetUserAsync(userEmail);

        if (oktaUser != null)
        {
            var isOlderThanOneDay = DateTime.UtcNow - oktaUser.Created > TimeSpan.FromDays(1);

            if (oktaUser.IsProvisioned && isOlderThanOneDay)
            {
                success = await oktaUserDirectory.ReactivateUserAsync(userEmail);
            }
            else if (oktaUser.IsActive)
            {
                success = true;
            }

            return new UserProvisioningStatus { Success = success };
        }
   
        success = await oktaUserDirectory.CreateUserAsync(userEmail, firstName, lastName);

        return new UserProvisioningStatus{ 
            Success = success, 
            FailureReason = !success ? "User could not be created" : string.Empty 
        };
    }
}
