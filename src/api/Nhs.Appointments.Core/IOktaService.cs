namespace Nhs.Appointments.Core;

public interface IOktaService
{
    public Task<UserProvisioningStatus> CreateIfNotExists(string userEmail, string? firstName, string? lastName);
}
