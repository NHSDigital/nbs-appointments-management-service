using System.Text.Json.Serialization;

namespace Nbs.MeshClient.Responses;

/// <summary>
/// Send Message Response
/// </summary>
public record SendMessageResponse
{
    /// <summary>
    /// Message Id
    /// </summary>
    [JsonPropertyName("message_id")] public string? MessageId { get; set; }
}
