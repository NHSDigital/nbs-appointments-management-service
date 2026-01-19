using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Nbs.MeshClient.Responses;

namespace Nbs.MeshClient.Errors;

/// <summary>
/// Mesh Error Response Handler
/// </summary>
public class MeshErrorResponseHandler(ILogger<MeshErrorResponseHandler> logger) : DelegatingHandler
{
    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        // Only handle known errors here so that other errors are handled by Refit
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken)
                .ConfigureAwait(false);
            if (error is not null)
            {
                throw new MeshErrorResponseException(error);
            }
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Could not parse MESH error response. Unexpected format.");
        }

        // Fall back to processing the remainder of the stack.
        return response;
    }
}
