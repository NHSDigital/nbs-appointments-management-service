using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Api.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.FeatureFlags;

[FeatureFile("./Scenarios/FeatureFlags/FeatureFlags.feature")]
public sealed class FeatureFlags : BaseFeatureSteps
{
    private GetFeatureFlagResponse _actualResponse;
    private HttpResponseMessage _response;
    private HttpStatusCode _statusCode;
    
    [When(@"I request the enabled state for feature flag '(.*)'")]
    public async Task FeatureFlagEnabled(string featureFlag)
    {
        _response = await Http.GetAsync(
            $"http://localhost:7071/api/feature-flag/{featureFlag}");
        _statusCode = _response.StatusCode;
        (_, _actualResponse) =
            await JsonRequestReader.ReadRequestAsync<GetFeatureFlagResponse>(await _response.Content.ReadAsStreamAsync());
    }

    [Then(@"the response should be 200 with enabled state '(true|false)'")]
    public async Task AssertEnabled(bool enabled)
    {
        _statusCode.Should().Be(HttpStatusCode.OK);
        _actualResponse.Enabled.Should().Be(enabled);
    }
}
