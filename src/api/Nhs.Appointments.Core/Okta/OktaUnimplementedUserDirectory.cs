namespace Nhs.Appointments.Core.Okta;

public class OktaUnimplementedUserDirectory : IOktaUserDirectory
{
    public Task<bool> ReactivateUserAsync(string user) => throw new NotImplementedException();

    public Task<bool> CreateUserAsync(string user, string firstName, string lastName) =>
        throw new NotImplementedException();

    public Task<OktaUserResponse> GetUserAsync(string user) => throw new NotImplementedException();
}
