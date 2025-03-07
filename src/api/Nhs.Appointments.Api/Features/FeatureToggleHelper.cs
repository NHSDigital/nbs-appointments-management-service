using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Features;

public class FeatureToggleHelper(IFeatureManager featureManager) : IFeatureToggleHelper
{
    public async Task<bool> IsFeatureEnabledForFunction(string featureFlag, FunctionContext functionContext,
        ClaimsPrincipal principal, IRequestInspector requestInspector)
    {
        var siteIds = (await requestInspector.GetSiteIds(await functionContext.GetHttpRequestDataAsync())).ToList();
        var targetingContext = new TargetingContext
        {
            UserId = principal.Claims.GetUserEmail(), Groups = siteIds.Select(x => $"Site:{x}")
        };
        return await featureManager.IsEnabledAsync(featureFlag, targetingContext);
    }
    
    public async Task<bool> IsFeatureEnabled(string featureFlag, string userId, string siteId)
    {
        var targetingUser = userId.IsNullOrWhiteSpace() ? null : userId;
        var targetingSite = siteId.IsNullOrWhiteSpace() ? null : new List<string>{ $"Site:{siteId}" };
        
        var targetingContext = new TargetingContext
        {
            UserId = targetingUser, Groups = targetingSite
        };
        
        return await featureManager.IsEnabledAsync(featureFlag, targetingContext);
    }
}
