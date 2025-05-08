namespace Nhs.Appointments.Core;

public interface IEmailWhitelistStore
{
    public Task<IEnumerable<string>> GetWhitelistedEmails();
}
