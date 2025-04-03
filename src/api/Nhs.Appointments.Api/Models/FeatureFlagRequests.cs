namespace Nhs.Appointments.Api.Models;

/// <summary>
/// Request the enabled state for the provided flag.
/// </summary>
/// <param name="Flag">The flag to check</param>
public record GetFeatureFlagRequest(string Flag);

/// <summary>
/// Response for the flag request
/// </summary>
/// <param name="Enabled"></param>
public record GetFeatureFlagResponse(bool Enabled);

/// <summary>
/// Local integration request to override a specific feature flag to return a specific enabled state
/// </summary>
/// <param name="Flag">The flag to override</param>
/// <param name="Enabled">The new overridden enabled value</param>
public record SetLocalFeatureFlagOverrideRequest(string Flag, bool Enabled);
