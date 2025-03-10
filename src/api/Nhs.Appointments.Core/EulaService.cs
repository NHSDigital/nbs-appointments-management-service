namespace Nhs.Appointments.Core;

public interface IEulaService
{
    Task<EulaVersion> GetEulaVersionAsync();
    Task ConsentToEula(string userId);
}

public class EulaService(IEulaStore eulaStore, IUserStore userStore) : IEulaService
{
    public async Task<EulaVersion> GetEulaVersionAsync()
    {
        return await eulaStore.GetLatestVersion();
    }

    public async Task ConsentToEula(string userId)
    {
        var latestVersion = await GetEulaVersionAsync();

        await userStore.RecordEulaAgreementAsync(userId, latestVersion.VersionDate);
    }
}
