namespace Nhs.Appointments.Core;

public interface IUserDirectory
{
    public Task<UserProvisioningStatus> CreateIfNotExists(string userEmail, string? firstName, string? lastName);
}
