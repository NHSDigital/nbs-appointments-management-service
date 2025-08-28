using FluentAssertions;
using Nhs.Appointments.Api.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.NoCache;

[FeatureFile("./Scenarios/NoCache/NoCache.feature")]
public sealed class NoCacheSteps : BaseFeatureSteps
{
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    private List<Core.Booking> _actualResponse;

    [When(@"I query for a site")]
    public async Task QuerySite()
    {
        _response = await Http.GetAsync($"http://localhost:7071/api/booking?nhsNumber={NhsNumber}");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) = await JsonRequestReader.ReadRequestAsync<List<Core.Booking>>(await _response.Content.ReadAsStreamAsync());
        
    }

    [Then("the call should be 200 with no cache headers")]
    public async Task AssertHeaders()
    {
        var headers = _response.Headers; 

        headers.CacheControl.NoCache.Should().BeTrue();
        headers.CacheControl.NoStore.Should().BeTrue();
        headers.CacheControl.MaxAge.Should().Be(new TimeSpan(0, 0, 0));
        headers.Pragma.First().Name.Should().Be("no-cache");

        _statusCode.Should().Be(HttpStatusCode.OK);
    }
}
