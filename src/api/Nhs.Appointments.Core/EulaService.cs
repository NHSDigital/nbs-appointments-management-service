using Microsoft.FeatureManagement;

namespace Nhs.Appointments.Core;

public interface IEulaService
{
    Task<EulaVersion> GetEulaVersionAsync();
    Task ConsentToEula(string userId);
}

public class EulaService(IEulaStore eulaStore, IUserStore userStore, IFeatureManager featureManager) : IEulaService
{
    public async Task<EulaVersion> GetEulaVersionAsync()
    {
        if (await featureManager.IsEnabledAsync(FeatureFlags.TestFeatureEnabled))
        {
            return new EulaVersion
            {
                VersionDate = DateOnly.Parse("2025-02-26"),
            };
        }
        
        return await eulaStore.GetLatestVersion();
    }

    public async Task ConsentToEula(string userId)
    {
        var latestVersion = await GetEulaVersionAsync();

        await userStore.RecordEulaAgreementAsync(userId, latestVersion.VersionDate);
    }
}
