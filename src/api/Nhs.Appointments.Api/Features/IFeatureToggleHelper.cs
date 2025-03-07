using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Features;

public interface IFeatureToggleHelper
{
    /// <summary>
    /// Returns whether the provided featureFlag is enabled in the current configuration
    /// Checks against the user that requests the function, and against a potential targeted site via the provided SiteRequestInspector
    /// </summary>
    /// <param name="featureFlag"></param>
    /// <param name="functionContext"></param>
    /// <param name="principal"></param>
    /// <param name="requestInspector"></param>
    /// <returns></returns>
    Task<bool> IsFeatureEnabledForFunction(string featureFlag, FunctionContext functionContext,
        ClaimsPrincipal principal, IRequestInspector requestInspector);

    /// <summary>
    /// Returns whether the provided featureFlag is enabled in the current configuration
    /// Will check against site and user filters if provided.
    /// </summary>
    /// <param name="featureFlag"></param>
    /// <param name="userId"></param>
    /// <param name="siteId"></param>
    /// <returns></returns>
    Task<bool> IsFeatureEnabled(string featureFlag, string userId, string siteId);
}
