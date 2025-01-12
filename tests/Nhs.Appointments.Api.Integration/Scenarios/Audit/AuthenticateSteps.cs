using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.Authentication;

[FeatureFile("./Scenarios/Audit/Authenticate.feature")]
public sealed class AuthenticateSteps : BaseFeatureSteps
{
    private HttpResponseMessage _response;

    [When(@"I fire an authenticate request")]
    public async Task AuthenticateRequest()
    {
        _response = await Http.GetAsync("http://localhost:7071/api/authenticate");
    }

    [Then("the call should be successful")]
    public void AssertHttpOk()
    {
        _response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
