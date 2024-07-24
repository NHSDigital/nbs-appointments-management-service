using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Moq;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Tests.Auth;

public class SiteFromScopeInspectorTests
{
    private readonly Mock<FunctionContext> _functionContext = new();
    private readonly SiteFromScopeInspector _sut = new();

    [Fact]
    public async Task GetSiteId_ReturnsSiteId_WhenPresentInBodyAsScope()
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.SetBody("{\"scope\": \"site:1234\"}");
        var actualResult = await _sut.GetSiteId(httpRequest);
        actualResult.Should().Be("1234");
    }
    
    [Fact]
    public async Task GetSiteId_ReturnsEmptyString_WhenScopeDoesNotIncludeSite()
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.SetBody("{\"scope\": \"1234\"}");
        var actualResult = await _sut.GetSiteId(httpRequest);
        actualResult.Should().Be("");
    }
    
    [Theory]
    [InlineData("this is not json")]
    [InlineData("")]
    [InlineData("{\"info\": \"there is no site property\"}")]
    public async Task GetSiteId_ReturnsEmpty_WhenBodyIsInvalid(string body)
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.SetBody(body);
        var actualResult = await _sut.GetSiteId(httpRequest);
        actualResult.Should().Be("");
    }   
}
