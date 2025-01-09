using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Core.Inspectors;

namespace Nhs.Appointments.Api.Tests.Auth;

public class SiteFromQueryStringInspectorTests
{
    private readonly Mock<FunctionContext> _functionContext = new();
    private readonly SiteFromQueryStringInspector _sut = new();

    [Fact]
    public async Task GetSiteId_ReturnsSiteId_WhenPresentInQuery()
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);
        httpRequest.Query.Add("site", "1234");
        var actualResult = await _sut.GetSiteIds(httpRequest);
        actualResult.Should().BeEquivalentTo(["1234"]);
    }

    [Fact]    
    public async Task GetSiteId_ReturnsEmpty_WhenSiteNotInQuery()
    {
        var httpRequest = new TestHttpRequestData(_functionContext.Object);        
        var actualResult = await _sut.GetSiteIds(httpRequest);
        actualResult.Should().BeEmpty();
    }
}
