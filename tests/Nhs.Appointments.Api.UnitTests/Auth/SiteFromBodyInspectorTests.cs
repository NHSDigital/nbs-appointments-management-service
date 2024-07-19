using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Moq;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Tests.Auth;

public class SiteFromBodyInspectorTests
{
    private readonly Mock<FunctionContext> _functionContext = new();
    private readonly SiteFromBodyInspector _sut = new();

    [Fact]
    public async Task GetSiteId_ReturnsSiteId_WhenPresentInBody()
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.SetBody("{\"site\": \"1234\"}");
        var actualResult = await _sut.GetSiteId(httpRequest);
        actualResult.Should().Be("1234");
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
