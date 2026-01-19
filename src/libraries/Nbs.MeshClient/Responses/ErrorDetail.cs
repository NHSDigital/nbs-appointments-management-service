using System.Text.Json.Serialization;

namespace Nbs.MeshClient.Responses;

/// <summary>
/// Error Detail
/// </summary>
public record ErrorDetail
{
    /// <summary>
    /// Event
    /// </summary>
    public required string Event { get; set; }
    
    /// <summary>
    /// Code
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Message
    /// </summary>
    [JsonPropertyName("msg")] public required string Message { get; set; }

    /// <summary>
    /// Stringify
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var delimiter = string.IsNullOrEmpty(Event) && string.IsNullOrEmpty(Code) ? string.Empty : " - ";
        return $"{Event} {Code}{delimiter}{Message}".Trim();
    }
}
