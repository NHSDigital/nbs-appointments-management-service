using System.Globalization;

namespace Nhs.Appointments.ApiClient
{
    public class RequestSigningHandler(TimeProvider timeProvider, string signingKey) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestTimestamp = timeProvider.GetUtcNow().ToString("o", CultureInfo.InvariantCulture);
            var signature = await RequestSigner.SignRequestAsync(request, requestTimestamp, signingKey);

            request.Headers.Authorization = new("Signed", signature);
            request.Headers.Add("RequestTimestamp", requestTimestamp);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }        
    }
}
