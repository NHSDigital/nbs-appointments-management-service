using Microsoft.Extensions.Configuration;
using Nhs.Appointments.Core.Features;

namespace BookingGenerator;

internal class FeatureToggleHelper(IConfiguration configuration) : IFeatureToggleHelper
{
    public Task<bool> IsFeatureEnabled(string featureFlag) => Task.FromResult(configuration.GetValue<bool>($"FEATURES:{featureFlag}"));

    public Task<bool> IsFeatureEnabledForSite(string featureFlag, string siteId) => throw new NotImplementedException();

    public Task<bool> IsFeatureEnabledForUser(string featureFlag, string userId) => throw new NotImplementedException();

    public void SetOverride(string flagName, bool enabled) => throw new NotImplementedException();

    public void ClearOverrides() => throw new NotImplementedException();
}
