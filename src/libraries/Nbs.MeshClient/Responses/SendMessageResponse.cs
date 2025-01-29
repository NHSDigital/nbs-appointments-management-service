using System.Text.Json.Serialization;

namespace Nbs.MeshClient.Responses
{
    public record SendMessageResponse
    {
        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }
    }
}
