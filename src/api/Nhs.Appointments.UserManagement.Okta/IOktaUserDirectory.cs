using Okta.Sdk.Model;

namespace Nhs.Appointments.UserManagement.Okta;
public interface IOktaUserDirectory
{
    Task<UserGetSingleton?> GetUserAsync(string user);
    Task<bool> ReactivateUserAsync(string user);
    Task<bool> CreateUserAsync(string user, string? firstName, string? lastName);
}
