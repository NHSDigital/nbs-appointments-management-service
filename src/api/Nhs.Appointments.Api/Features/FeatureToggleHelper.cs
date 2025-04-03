using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Api.Features;

public class FeatureToggleHelper(IFeatureManager featureManager, IConfigurationRefresher configRefresher) : IFeatureToggleHelper
{
    private readonly ConcurrentDictionary<string, bool?> _overrides = new();
    
    public async Task<bool> IsFeatureEnabled(string featureFlag)
    {
        if (string.IsNullOrEmpty(featureFlag))
        {
            throw new ArgumentException("FeatureFlag cannot be null or empty.");
        }
        
        if (_overrides.TryGetValue(featureFlag, out var overrideValue) && overrideValue.HasValue)
        {
            return overrideValue.Value;
        }
        
        // fire and forget to not block execution
        // means a slight delay in applying the very latest configuration, but its worth it to not block every time this is invoked
        _ = configRefresher.RefreshAsync();
        return await featureManager.IsEnabledAsync(featureFlag);
    }

    public async Task<bool> IsFeatureEnabledForUser(string featureFlag, string userId)
    {
        if (string.IsNullOrEmpty(featureFlag))
        {
            throw new ArgumentException("FeatureFlag cannot be null or empty.");
        }
        
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("UserId cannot be null or empty.");
        }
        
        if (_overrides.TryGetValue(featureFlag, out var overrideValue) && overrideValue.HasValue)
        {
            return overrideValue.Value;
        }

        var targetingContext = new TargetingContext { UserId = userId };

        // fire and forget to not block execution
        // means a slight delay in applying the very latest configuration, but its worth it to not block every time this is invoked
        _ = configRefresher.RefreshAsync();
        return await featureManager.IsEnabledAsync(featureFlag, targetingContext);
    }

    public async Task<bool> IsFeatureEnabledForSite(string featureFlag, string siteId)
    {
        if (string.IsNullOrEmpty(featureFlag))
        {
            throw new ArgumentException("FeatureFlag cannot be null or empty.");
        }
        
        if (string.IsNullOrEmpty(siteId))
        {
            throw new ArgumentException("SiteId cannot be null or empty.");
        }
        
        if (_overrides.TryGetValue(featureFlag, out var overrideValue) && overrideValue.HasValue)
        {
            return overrideValue.Value;
        }

        var targetingContext = new TargetingContext { Groups = [$"Site:{siteId}"] };

        // fire and forget to not block execution
        // means a slight delay in applying the very latest configuration, but its worth it to not block every time this is invoked
        _ = configRefresher.RefreshAsync();
        return await featureManager.IsEnabledAsync(featureFlag, targetingContext);
    }
    
    [Obsolete("Only for use in local testing purposes.")]
    public void SetOverride(string flagName, bool enabled) => _overrides[flagName] = enabled;

    [Obsolete("Only for use in local testing purposes.")]
    public void ClearOverrides() => _overrides.Clear();
}
