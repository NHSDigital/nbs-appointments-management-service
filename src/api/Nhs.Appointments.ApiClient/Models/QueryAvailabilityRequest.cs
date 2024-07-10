using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record QueryAvailabilityRequest(
    [property: JsonPropertyName("sites")]
    string[] Sites,
    [property: JsonPropertyName("service")]
    string Service,
    [property: JsonPropertyName("from")]
    string From,
    [property: JsonPropertyName("until")]
    string Until,
    [property: JsonPropertyName("queryType")]
    QueryType QueryType
);
}
