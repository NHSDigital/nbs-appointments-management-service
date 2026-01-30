using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.NoCache;

[FeatureFile("./Scenarios/NoCache/NoCache.feature")]
public sealed class NoCacheSteps : BaseFeatureSteps
{
    private HttpStatusCode _statusCode;

    [When(@"I query for a site")]
    public async Task QuerySite()
    {
        _response = await GetHttpClientForTest().GetAsync($"http://localhost:7071/api/booking?nhsNumber={NhsNumber}");
        _statusCode = _response.StatusCode;
    }

    [Then("the call should be 200 with no cache headers")]
    public void AssertHeaders()
    {
        var headers = _response.Headers;

        headers.CacheControl.NoCache.Should().BeTrue();
        headers.CacheControl.NoStore.Should().BeTrue();
        headers.CacheControl.MaxAge.Should().Be(new TimeSpan(0, 0, 0));
        headers.Pragma.First().Name.Should().Be("no-cache");

        _statusCode.Should().Be(HttpStatusCode.OK);
    }
}
