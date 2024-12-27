using Newtonsoft.Json;

namespace BookingsDataExtracts.Documents;

public record Location
(
    [property:JsonProperty("type")]
    string Type,
    [property:JsonProperty("coordinates")]
    double[] Coordinates
);
