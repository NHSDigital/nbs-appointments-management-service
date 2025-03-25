namespace Nhs.Appointments.Api.Models;

/// <summary>
/// Request the state for the provided flag.
/// </summary>
/// <param name="Flag">The flag to check against</param>
public record FeatureFlagRequest(string Flag);

/// <summary>
/// Test request to override a specific feature flag 
/// </summary>
/// <param name="Flag">The flag to check against</param>
/// <param name="Enabled">The new overridden value</param>
public record SetFeatureFlagOverrideRequest(string Flag, bool Enabled);
