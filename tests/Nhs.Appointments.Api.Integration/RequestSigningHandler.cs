using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Integration;

public class RequestSigningHandler(IRequestSigner signer) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await signer.SignRequestAsync(request);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
