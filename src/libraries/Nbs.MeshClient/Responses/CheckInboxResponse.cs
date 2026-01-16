namespace Nbs.MeshClient.Responses;

/// <summary>
/// Check Inbox Response
/// </summary>
public record CheckInboxResponse
{
    /// <summary>
    /// Messages
    /// </summary>
    public IReadOnlyCollection<string> Messages { get; set; } = Array.Empty<string>();
    /// <summary>
    /// Links
    /// </summary>
    public CollectionLinks? Links { get; set; }
}
