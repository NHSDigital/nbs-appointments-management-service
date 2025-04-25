namespace Nhs.Appointments.Core.Okta;

public interface IOktaUserDirectory
{
    Task<OktaUserResponse?> GetUserAsync(string user);
    Task<bool> ReactivateUserAsync(string user);
    Task<bool> CreateUserAsync(string user, string? firstName, string? lastName);
}
