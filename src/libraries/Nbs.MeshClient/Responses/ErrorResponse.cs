namespace Nbs.MeshClient.Responses;

/// <summary>
/// Error Response
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// Detail
    /// </summary>
    public required IReadOnlyCollection<ErrorDetail> Detail { get; set; }

    /// <summary>
    /// Stringify
    /// </summary>
    public override string ToString()
    {
        return string.Join(", ", Detail);
    }
}
