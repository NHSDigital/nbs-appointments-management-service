using Nhs.Appointments.Core.Users;

namespace Nhs.Appointments.Core.Okta;

public interface IOktaService
{
    public Task<UserProvisioningStatus> CreateIfNotExists(string userEmail, string firstName, string lastName);
}
