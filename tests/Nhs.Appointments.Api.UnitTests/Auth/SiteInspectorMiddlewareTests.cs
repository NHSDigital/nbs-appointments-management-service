using System.Collections.Specialized;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Moq;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Tests.Auth;

public class SiteInspectorMiddlewareTests
{
    private readonly Mock<FunctionContext> _functionContext = new();
    private readonly Mock<FunctionExecutionDelegate> _functionExecutionDelegate = new();
    private readonly SiteInspectorMiddleware _sut = new();

    [Fact]
    public async Task Invoke_PopulatesSiteId_WhenSiteInRequestQueryString()
    {
        var itemsDictionary = new Dictionary<object, object>();
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.Query.Add("site", "1");
        ConfigureMocks(httpRequest);
        _functionContext.Setup(x => x.Items).Returns(itemsDictionary);
        
        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);
        itemsDictionary.Should().Contain("siteId", "1");
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }
    
    [Fact]
    public async Task Invoke_PopulatesSiteId_WhenSiteInBody()
    {
        var itemsDictionary = new Dictionary<object, object>();
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        ConfigureMocks(httpRequest, "{\"site\": \"1\"}");
        _functionContext.Setup(x => x.Items).Returns(itemsDictionary);
        
        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);
        itemsDictionary.Should().Contain("siteId", "1");
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }
    
    [Fact]
    public async Task Invoke_ProcessesRequest_WhenNoSiteInRequestQueryStringAndNoRequestBody()
    {
        var itemsDictionary = new Dictionary<object, object>();
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        ConfigureMocks(httpRequest, "{\"someProperty\": \"someValue\"}");
        _functionContext.Setup(x => x.Items).Returns(itemsDictionary);
        
        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);
        itemsDictionary.Should().BeEmpty();
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }
    
    [Fact]
    public async Task Invoke_ProcessesRequest_WhenNoSiteInBody()
    {
        var itemsDictionary = new Dictionary<object, object>();
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        ConfigureMocks(httpRequest);
        _functionContext.Setup(x => x.Items).Returns(itemsDictionary);
        
        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);
        itemsDictionary.Should().BeEmpty();
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }
    
    [Fact]
    public async Task Invoke_ProcessesRequest_WhenBodyIsNotValidJson()
    {
        var itemsDictionary = new Dictionary<object, object>();
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        ConfigureMocks(httpRequest, "ABC123");
        _functionContext.Setup(x => x.Items).Returns(itemsDictionary);
        
        await _sut.Invoke(_functionContext.Object, _functionExecutionDelegate.Object);
        itemsDictionary.Should().BeEmpty();
        _functionExecutionDelegate.Verify(x => x(_functionContext.Object), Times.Once);
    }
    
    private void ConfigureMocks(TestHttpRequestData httpRequest, string body = null)
    {
        var mockHttpRequestDataFeature = new Mock<IHttpRequestDataFeature>();
        mockHttpRequestDataFeature.Setup(x => x.GetHttpRequestDataAsync(It.IsAny<FunctionContext>())).ReturnsAsync(httpRequest);

        var mockFeatures = new Mock<IInvocationFeatures>();
        mockFeatures.Setup(x => x.Get<IHttpRequestDataFeature>()).Returns(mockHttpRequestDataFeature.Object);

        httpRequest.SetBody(body);
        
        _functionContext.Setup(x => x.Features).Returns(mockFeatures.Object);
    }

    private class TestHttpRequestData(FunctionContext functionContext) : HttpRequestData(functionContext)
    {
        // This is required so that the request data extension method works
        private HttpRequest _httpRequest = new DefaultHttpContext().Request;
        
        private string _body;
        
        private readonly NameValueCollection _query = new();

        public override Stream Body => GetBodyStream();

        public override HttpHeadersCollection Headers => throw new NotImplementedException();

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
    
}
