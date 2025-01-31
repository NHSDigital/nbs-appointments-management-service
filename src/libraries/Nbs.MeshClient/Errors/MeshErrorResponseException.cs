using Nbs.MeshClient.Responses;

namespace Nbs.MeshClient.Errors
{
    public class MeshErrorResponseException(HttpResponseMessage response, ErrorResponse content) : Exception
    {
        public HttpResponseMessage Response { get; } = response;
        public ErrorResponse Content { get; } = content;

        public override string Message => $"Error response received from MESH: {Content}";
    }
}
