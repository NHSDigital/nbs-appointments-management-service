using Microsoft.Extensions.Logging;
using Okta.Sdk.Api;
using Okta.Sdk.Model;


namespace Nhs.Appointments.UserManagement.Okta;
public class OktaUserDirectory : IOktaUserDirectory
{
    private readonly UserApi _userApi;
    private readonly ILogger<OktaUserDirectory> _logger;

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
            _logger.LogInformation($"Failed to create okta, user: {user}, firstName: {firstName}, lastName: {lastName}!");
            return false;
        }
    }

    public async Task<UserGetSingleton?> GetUserAsync(string user){
        try
        {
            return await _userApi.GetUserAsync(user);
        }
        catch(Exception ex)
        {
            _logger.LogInformation($"User not found in okta directory: {user}!" );
            return null;
        }
    }
}
