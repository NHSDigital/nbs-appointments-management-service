using System.Net.Http.Headers;

namespace Nbs.MeshClient.Auth;

/// <summary>
/// MeshAuthorizationHandler
/// </summary>
public class MeshAuthorizationHandler(IMeshAuthorizationTokenGenerator tokenGenerator) : DelegatingHandler
{
    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        AddTokenToRequest(request);
        return base.SendAsync(request, cancellationToken);
    }

    private void AddTokenToRequest(HttpRequestMessage request)
    {
        var token = tokenGenerator.GenerateAuthorizationToken();
        request.Headers.Authorization = new AuthenticationHeaderValue("NHSMESH", token);
    }
}
