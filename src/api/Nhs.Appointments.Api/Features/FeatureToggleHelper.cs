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
        var siteIds = (await requestInspector.GetSiteIds(await functionContext.GetHttpRequestDataAsync())).ToArray();
        return await IsFeatureEnabled(featureFlag, principal.Claims.GetUserEmail(), siteIds);
    }

    public async Task<bool> IsFeatureEnabled(string featureFlag, string userId, string[] sites)
    {
        var targetingUser = userId.IsNullOrWhiteSpace() ? null : userId;
        IEnumerable<string> targetingSites = null;

        if (sites != null && sites.Length != 0)
        {
            targetingSites = sites.Select(x => $"Site:{x}");
        }

        var targetingContext = new TargetingContext { UserId = targetingUser, Groups = targetingSites };

        return await featureManager.IsEnabledAsync(featureFlag, targetingContext);
    }
}
