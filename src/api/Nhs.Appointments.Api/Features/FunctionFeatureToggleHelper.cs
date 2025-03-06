using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Features;

public class FunctionFeatureToggleHelper(IFeatureManager featureManager) : IFunctionFeatureToggleHelper
{
    public async Task<bool> IsFeatureEnabled(string featureFlag, FunctionContext functionContext,
        ClaimsPrincipal principal, IRequestInspector requestInspector)
    {
        var siteIds = (await requestInspector.GetSiteIds(await functionContext.GetHttpRequestDataAsync())).ToList();
        var targetingContext = new TargetingContext
        {
            UserId = principal.Claims.GetUserEmail(), Groups = siteIds.Select(x => $"Site:{x}")
        };
        return await featureManager.IsEnabledAsync(featureFlag, targetingContext);
    }
}
