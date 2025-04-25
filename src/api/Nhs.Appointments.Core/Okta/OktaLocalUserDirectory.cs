namespace Nhs.Appointments.UserManagement.Okta;

public class OktaLocalUserDirectory : IOktaUserDirectory
{
    private bool UserAlreadyExistsOnImaginaryOktaServer(string user) => user.StartsWith("test.provisioned.okta.user");
    public async Task<bool> ReactivateUserAsync(string user) => await Task.FromResult(true);

    public async Task<bool> CreateUserAsync(string user, string? firstName, string? lastName) =>
        await Task.FromResult(!UserAlreadyExistsOnImaginaryOktaServer(user));

    public async Task<OktaUserResponse?> GetUserAsync(string user) => user.StartsWith("test.provisioned.okta.user")
        ? await Task.FromResult(new OktaUserResponse
        {
            Created = DateTimeOffset.Now.AddDays(-7), IsActive = true, IsProvisioned = true
        })
        : null;
}
