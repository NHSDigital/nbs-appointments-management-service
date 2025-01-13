using System.Net.Http.Headers;

namespace Nbs.MeshClient.Auth
{
    public class MeshAuthorizationHandler(IMeshAuthorizationTokenGenerator tokenGenerator) : DelegatingHandler
    {
        private readonly IMeshAuthorizationTokenGenerator _tokenGenerator = tokenGenerator;        

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AddTokenToRequest(request);
            return base.SendAsync(request, cancellationToken);
        }

        private void AddTokenToRequest(HttpRequestMessage request)
        {
            var token = _tokenGenerator.GenerateAuthorizationToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("NHSMESH", token);
        }
    }
}
