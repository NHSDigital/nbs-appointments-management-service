using System.Collections.Generic;

namespace Nhs.Appointments.Api.Models;

public record GetFeatureFlagsResponse(List<KeyValuePair<string, bool>> FeatureFlags);
