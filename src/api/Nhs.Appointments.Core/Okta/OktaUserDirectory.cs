using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Okta.Sdk.Api;
using Okta.Sdk.Client;
using Okta.Sdk.Model;

namespace Nhs.Appointments.Core.Okta;

public class OktaUserDirectory : IOktaUserDirectory
{
    private readonly ILogger<OktaUserDirectory> _logger;
    private readonly UserApi _userApi;

    public OktaUserDirectory(UserApi userApi, ILogger<OktaUserDirectory> logger)
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
        switch (userStatus)
        {
            // Okta account management isn't our concern, so treat states where admin action is needed
            // as being active as far as we're concerned
            case nameof(UserStatus.ACTIVE):
            case nameof(UserStatus.SUSPENDED):
            case nameof(UserStatus.RECOVERY):
            case nameof(UserStatus.PASSWORDEXPIRED):
            case nameof(UserStatus.LOCKEDOUT):
                return OktaUserStatus.Active;

            // A provisioned user needs to set their own password and activate their account to move to the Active state
            // The only workflow we can safely begin for them is "Deactivate User" ("Reactivate" calls this followed by "Activate")
            case nameof(UserStatus.PROVISIONED):
                return OktaUserStatus.Provisioned;

            // Is a user is Staged, Deprovisioned, or has never existed, it is safe to begin the "Activate User" workflow for them
            case nameof(UserStatus.STAGED):
            case nameof(UserStatus.DEPROVISIONED):
                return OktaUserStatus.Deactivated;

            default:
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
