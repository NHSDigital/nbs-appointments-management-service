using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Moq;
using Nhs.Appointments.Api.Auth;

namespace Nhs.Appointments.Api.Tests.Auth;
public class MultiSiteBodyRequestInspectorTests
{
    private readonly Mock<FunctionContext> _functionContext = new();
    private readonly MultiSiteBodyRequestInspector _sut = new();

    [Fact]
    public async Task GetSiteIds_ReturnsSiteIds_WhenPresentInBody()
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.SetBody("{\"sites\": [\"1234\", \"5678\"]}");
        var actualResult = await _sut.GetSiteIds(httpRequest);
        actualResult.Should().BeEquivalentTo(["1234", "5678"]);
    }

    [Theory]
    [InlineData("this is not json")]
    [InlineData("")]
    [InlineData("{\"info\": \"there is no site property\"}")]
    public async Task GetSiteId_ReturnsEmpty_WhenBodyIsInvalid(string body)
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.SetBody(body);
        var actualResult = await _sut.GetSiteIds(httpRequest);
        actualResult.Should().BeEmpty();
    }
}
