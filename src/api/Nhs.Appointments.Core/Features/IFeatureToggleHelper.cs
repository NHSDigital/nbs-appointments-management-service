namespace Nhs.Appointments.Core.Features;

public interface IFeatureToggleHelper
{
    /// <summary>
    ///     Check whether the provided featureFlag is enabled.
    ///     This method does not consider any targeting context (i.e. checking by user or site)
    /// </summary>
    /// <param name="featureFlag">The flag to verify</param>
    /// <returns>Boolean value to indicate if the flag is enabled</returns>
    Task<bool> IsFeatureEnabled(string featureFlag);

    /// <summary>
    ///     Check whether the provided featureFlag is enabled for the provided site.
    ///     If the flag has no specific site targeting, it will return the global state of the flag
    /// </summary>
    /// <param name="featureFlag">The flag to verify</param>
    /// <param name="siteId">The site to add to the feature filter targeting context</param>
    /// <returns></returns>
    Task<bool> IsFeatureEnabledForSite(string featureFlag, string siteId);
    
    /// <summary>
    ///     Check whether the provided featureFlag is enabled for the provided user.
    ///     If the flag has no specific user targeting, it will return the global state of the flag
    /// </summary>
    /// <param name="featureFlag">The flag to verify</param>
    /// <param name="userId">The user to add to the feature filter targeting context</param>
    /// <returns></returns>
    Task<bool> IsFeatureEnabledForUser(string featureFlag, string userId);
}
