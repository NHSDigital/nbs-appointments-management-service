using Microsoft.Extensions.Logging;
using Nbs.MeshClient.Responses;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nbs.MeshClient.Errors
{
    public class MeshErrorResponseHandler(ILogger<MeshErrorResponseHandler> _logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return response;

            // Only handle known errors here so that other errors are handled by Refit
            try
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken).ConfigureAwait(false);
                if (error is not null)
                    throw new MeshErrorResponseException(response, error);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Could not parse MESH error response. Unexpected format.");
            }

            // Fall back to processing the remainder of the stack.
            return response;
        }
    }
}
