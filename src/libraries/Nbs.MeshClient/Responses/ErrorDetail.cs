using System.Text.Json.Serialization;

namespace Nbs.MeshClient.Responses
{
    public record ErrorDetail
    {
        public required string Event { get; set; }
        public required string Code { get; set; }

        [JsonPropertyName("msg")]
        public required string Message { get; set; }

        public override string ToString()
        {
            var delimiter = string.IsNullOrEmpty(Event) && string.IsNullOrEmpty(Code) ? string.Empty : " - ";
            return $"{Event} {Code}{delimiter}{Message}".Trim();
        }
    }
}
