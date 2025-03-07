using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Features;

public interface IFeatureToggleHelper
{
    /// <summary>
    /// Check whether the provided featureFlag is enabled for the calling Function.
    /// Checks if the flag is enabled for the user that invokes the function.
    /// Ability to check against a potential targeted site for the function, via the provided SiteRequestInspector.
    /// </summary>
    /// <param name="featureFlag">The flag to verify is enabled</param>
    /// <param name="functionContext">The function invocation context</param>
    /// <param name="principal">The user context principal for the function invocation</param>
    /// <param name="requestInspector">The site request inspector to try and extract any relevant siteIds, to be added to targeting context</param>
    /// <returns></returns>
    Task<bool> IsFeatureEnabledForFunction(string featureFlag, FunctionContext functionContext,
        ClaimsPrincipal principal, IRequestInspector requestInspector);

    /// <summary>
    /// Check whether the provided featureFlag is enabled.
    /// Will check if the flag is enabled for the site and user parameters, if provided.
    /// </summary>
    /// <param name="featureFlag">The flag to verify is enabled</param>
    /// <param name="userId">The userId, if provided, to add to the feature filter targeting context</param>
    /// <param name="siteId">The siteId, if provided, to add to the feature filter targeting context</param>
    /// <returns></returns>
    Task<bool> IsFeatureEnabled(string featureFlag, string userId, string siteId);
}
