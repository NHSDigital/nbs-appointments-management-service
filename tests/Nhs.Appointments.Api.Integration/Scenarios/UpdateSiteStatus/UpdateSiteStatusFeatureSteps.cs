using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Persistance.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UpdateSiteStatus;
public abstract class UpdateSiteStatusFeatureSteps(string flag, bool enabled) : FeatureToggledSteps(flag, enabled)
{
    private HttpResponseMessage Response { get; set; }
    private SiteStatus UpdatedSiteStatus;

    [When(@"I update the site status to '(.*)'")]
    public async Task UpdateSiteStatus(string status)
    {
        UpdatedSiteStatus = Enum.Parse<SiteStatus>(status);

        var payload = new
        {
            site = _testId,
            status = UpdatedSiteStatus
        };

        Response = await Http.PostAsJsonAsync("http://localhost:7071/api/site-status", payload);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Then("the site should have an updated site status")]
    public async Task AssertUpdatedSiteStatus()
    {
        Response.EnsureSuccessStatusCode();

        var actualResult = await Client.GetContainer("appts", "core_data").ReadItemAsync<SiteDocument>(_testId.ToString(), new PartitionKey("site"));
        actualResult.Resource.Status.Should().Be(UpdatedSiteStatus);
    }

    [Collection("UpdateSiteStatusToggle")]
    [FeatureFile("./Scenarios/UpdateSiteStatus/UpdateSiteStatus_Disabled.feature")]
    public class UpdateSiteStatusFeatureSteps_SiteStatusDisabled() : UpdateSiteStatusFeatureSteps(Flags.SiteStatus, false);

    [Collection("UpdateSiteStatusToggle")]
    [FeatureFile("./Scenarios/UpdateSiteStatus/UpdateSiteStatus_Enabled.feature")]
    public class UpdateSiteStatusFeatureSteps_SiteStatusEnabled() : UpdateSiteStatusFeatureSteps(Flags.SiteStatus, true);
}
