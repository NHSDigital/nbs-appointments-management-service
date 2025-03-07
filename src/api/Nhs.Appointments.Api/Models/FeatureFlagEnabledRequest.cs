namespace Nhs.Appointments.Api.Models;

/// <summary>
/// Request the enabled state for the provided flag. SiteId and UserId optional.
/// </summary>
/// <param name="Flag">The flag to check against</param>
/// <param name="SiteId">Optional check if the flag is enabled for a specific site</param>
/// <param name="UserOverrideId">Override the requesting API user with a custom userId</param>
public record FeatureFlagEnabledRequest(string Flag, string SiteId, string UserOverrideId);
