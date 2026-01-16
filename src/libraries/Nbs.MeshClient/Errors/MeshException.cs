namespace Nbs.MeshClient.Errors;

/// <summary>
/// Mesh Exception
/// </summary>
public class MeshException : Exception
{
    /// <inheritdoc />
    public MeshException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public MeshException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
