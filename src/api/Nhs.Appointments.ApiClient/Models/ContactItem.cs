
using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models;

public record ContactItem(
[property: JsonPropertyName("type")]
string Type,
[property: JsonPropertyName("value")]
string Value
)
{ }
