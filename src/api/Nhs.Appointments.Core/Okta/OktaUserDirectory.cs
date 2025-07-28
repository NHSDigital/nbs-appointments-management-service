using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Okta.Sdk.Api;
using Okta.Sdk.Client;
using Okta.Sdk.Model;

namespace Nhs.Appointments.Core.Okta;

public class OktaUserDirectory : IOktaUserDirectory
{
    private readonly ILogger<OktaUserDirectory> _logger;
    private readonly IUserApi _userApi;

    public OktaUserDirectory(IUserApi userApi, ILogger<OktaUserDirectory> logger)
    {
        _userApi = userApi;
        _logger = logger;
    }

    public async Task<bool> ReactivateUserAsync(string user)
    {
        try
        {
            await _userApi.ReactivateUserAsync(user, true);
            return true;
        }
        catch (ApiException ex)
        {
            var errorContent = JsonConvert.DeserializeObject<OktaErrorContent>(ex.ErrorContent.ToString());
            if (errorContent.ErrorCode == "E0000006" && errorContent.ErrorSummary ==
                "You do not have permission to perform the requested action")
            {
                _logger.LogError(
                    "Erroneously lacked permissions to de/reactivate a user. Known bug APPT-898. Swallowing this error until this bug is resolved. Okta error code: {errorCode}, Okta error summary: {errorSummary}",
                    errorContent.ErrorCode, errorContent.ErrorSummary);
                return true; // Should return false on error once APPT-898 is resolved
            }


            _logger.LogInformation($"Failed to reactivate okta user: {user}!");
            return false;
        }
        catch (Exception)
        {
            _logger.LogInformation($"Failed to reactivate okta user: {user}!");
            return false;
        }
    }

    public async Task<bool> CreateUserAsync(string user, string? firstName, string? lastName)
    {
        try
        {
            var createdUser = await _userApi.CreateUserAsync(new CreateUserRequest
            {
                Profile = new UserProfile { Email = user, Login = user, FirstName = firstName, LastName = lastName }
            });
            return createdUser != null;
        }
        catch (Exception)
        {
            _logger.LogInformation(
                $"Failed to create okta, user: {user}, firstName: {firstName}, lastName: {lastName}!");
            return false;
        }
    }


    public async Task<OktaUserResponse> GetUserAsync(string user)
    {
        try
        {
            var oktaUser = await _userApi.GetUserAsync(user);

            return oktaUser == null
                ? null
                : new OktaUserResponse { Created = oktaUser.Created, Status = GetUserStatusForMya(oktaUser.Status) };
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"User not found in okta directory: {user}!");
            return null;
        }
    }

    /// <summary>
    ///     See https://help.okta.com/en-us/content/topics/users-groups-profiles/usgp-end-user-states.htm
    /// </summary>
    /// <param name="userStatus"></param>
    /// <returns></returns>
    private OktaUserStatus GetUserStatusForMya(UserStatus userStatus)
    {
        // Okta's UserStatus class defines static fields like `LOCKEDOUT` but assigns them string values like "LOCKED_OUT".
        // Because the field name and value don't always match (e.g., LOCKEDOUT => "LOCKED_OUT"),
        // we cannot rely on enum field names or use nameof(...). Instead, we match on userStatus.Value,
        // comparing directly against known status string values returned by Okta's API.
        // This approach ensures compatibility with Okta's inconsistent enum-style implementation.

        switch (userStatus.Value)
        {
            // Okta account management isn't our concern, so treat states where admin action is needed
            // as being active as far as we're concerned
            case "ACTIVE":
            case "SUSPENDED":
            case "RECOVERY":
            case "PASSWORD_EXPIRED":
            case "LOCKED_OUT":
                return OktaUserStatus.Active;

            // A provisioned user needs to set their own password and activate their account to move to the Active state
            // The only workflow we can safely begin for them is "Deactivate User" ("Reactivate" calls this followed by "Activate")
            case "PROVISIONED":
                return OktaUserStatus.Provisioned;

            // Is a user is Staged, Deprovisioned, or has never existed, it is safe to begin the "Activate User" workflow for them
            case "STAGED":
            case "DEPROVISIONED":
                return OktaUserStatus.Deactivated;

            default:
                _logger.LogError("Failed to map okta user status: {userStatus}!", userStatus);
                return OktaUserStatus.Unknown;
        }
    }

    private class OktaErrorContent
    {
        public string ErrorCode { get; set; }
        public string ErrorSummary { get; set; }
        public string ErrorId { get; set; }
    }
}
