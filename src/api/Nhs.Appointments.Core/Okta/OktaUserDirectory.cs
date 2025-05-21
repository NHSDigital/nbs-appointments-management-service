using Microsoft.Extensions.Logging;
using Okta.Sdk.Api;
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
        catch (Exception ex)
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
        catch (Exception ex)
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

            // When a user is created they're briefly staged before being provisioned
            case nameof(UserStatus.STAGED):
            case nameof(UserStatus.PROVISIONED):
                return OktaUserStatus.Provisioned;

            // Okta docs do not say whether trying to CreateUser on a Deprovisioned user will succeed or not.
            // TODO: investigate this if we see errors for deleted users
            case nameof(UserStatus.DEPROVISIONED):
                return OktaUserStatus.Deactivated;

            default:
                return OktaUserStatus.Unknown;
        }
    }
}
