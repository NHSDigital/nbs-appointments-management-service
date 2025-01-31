namespace UserManagement.Okta;

public interface IUserDirectory
{
    public Task<UserProvisioningStatus> CreateIfNotExists(string user);
}
