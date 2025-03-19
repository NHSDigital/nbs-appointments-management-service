using System;
using System.Threading.Tasks;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

namespace Nhs.Appointments.Api.Features;

public class FeatureToggleHelper(IFeatureManager featureManager) : IFeatureToggleHelper
{
    public async Task<bool> IsFeatureEnabled(string featureFlag)
    {
        if (string.IsNullOrEmpty(featureFlag))
        {
            throw new ArgumentException("FeatureFlag cannot be null or empty.");
        }
        
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

        var targetingContext = new TargetingContext { UserId = userId };

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

        var targetingContext = new TargetingContext { Groups = [$"Site:{siteId}"] };

        return await featureManager.IsEnabledAsync(featureFlag, targetingContext);
    }
}
