namespace Nhs.Appointments.Api.Models;

/// <summary>
/// Request the state for the provided flag.
/// </summary>
/// <param name="Flag">The flag to check against</param>
public record FeatureFlagRequest(string Flag);
