using Nbs.MeshClient.Responses;

namespace Nbs.MeshClient.Errors;

/// <summary>
/// Mesh Error Response Exception
/// </summary>
public class MeshErrorResponseException(ErrorResponse content) : Exception
{
    /// <summary>
    /// ErrorResponse content
    /// </summary>
    private ErrorResponse Content { get; } = content;

    /// <summary>
    /// ErrorResponse content message
    /// </summary>
    public override string Message => $"Error response received from MESH: {Content}";
}
