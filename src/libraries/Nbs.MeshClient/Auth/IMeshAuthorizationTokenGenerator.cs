namespace Nbs.MeshClient.Auth;

/// <summary>
///     MeshAuthorizationTokenGenerator interface
/// </summary>
public interface IMeshAuthorizationTokenGenerator
{
    /// <summary>
    ///     GenerateAuthorizationToken
    /// </summary>
    string GenerateAuthorizationToken();
}
