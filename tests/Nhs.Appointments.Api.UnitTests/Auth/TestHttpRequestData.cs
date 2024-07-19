using System.Collections.Specialized;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Nhs.Appointments.Api.Tests.Auth;

public class TestHttpRequestData(FunctionContext functionContext) : HttpRequestData(functionContext)
{
    private readonly HttpHeadersCollection _headers = new();
    // This is required so that the request data extension method works
    private HttpRequest _httpRequest = new DefaultHttpContext().Request;

    private string _body;

    private readonly NameValueCollection _query = new();

    public override Stream Body => GetBodyStream();

    public override HttpHeadersCollection Headers => _headers;

    public override IReadOnlyCollection<IHttpCookie> Cookies => throw new NotImplementedException();

    public override Uri Url => throw new NotImplementedException();

    public override IEnumerable<ClaimsIdentity> Identities => throw new NotImplementedException();

    public override string Method => throw new NotImplementedException();

    public override NameValueCollection Query => _query;

    public override HttpResponseData CreateResponse()
    {
        throw new NotImplementedException();
    }

    public void SetBody(string body)
    {
        _body = body;
    }

    private Stream GetBodyStream()
    {
        if (string.IsNullOrEmpty(_body) == false)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_body));
        }
        else
        {
            return new MemoryStream();
        }
    }
}
