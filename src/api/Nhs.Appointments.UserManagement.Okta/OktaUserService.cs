using Okta.Sdk.Api;
using Okta.Sdk.Model;


namespace Nhs.Appointments.UserManagement.Okta;
public class OktaUserService : IOktaUserService
{
    private readonly UserApi _userApi;

    public OktaUserService(UserApi userApi)
    {
        _userApi = userApi;
    }

    public async Task<bool> ReactivateUserAsync(string user)
    {
        await _userApi.ReactivateUserAsync(user, true);
        return true;
    }

    public async Task<bool> CreateUserAsync(string user)
    {
        var createdUser = await _userApi.CreateUserAsync(new CreateUserRequest
        {
            Profile = new UserProfile { Email = user, Login = user, FirstName = "Not", LastName = "Applicable" },
        });
        return createdUser != null;
    }

    public async Task<UserGetSingleton?> GetUserAsync(string user){
        return await _userApi.GetUserAsync(user);
    }
}
