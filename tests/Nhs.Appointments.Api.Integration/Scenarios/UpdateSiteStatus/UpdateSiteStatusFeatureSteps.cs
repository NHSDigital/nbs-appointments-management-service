using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Persistance.Models;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.UpdateSiteStatus;

[FeatureFile("./Scenarios/UpdateSiteStatus/UpdateSiteStatus.feature")]
public class UpdateSiteStatusFeatureSteps : BaseFeatureSteps
{
    private HttpResponseMessage Response { get; set; }
    private SiteStatus UpdatedSiteStatus;

    [When(@"I update the site status for the default site to '(.*)'")]
    public async Task UpdateSiteStatus(string status)
    {
        object payload;

        if (Enum.TryParse(status, out UpdatedSiteStatus))
        {
            payload = new
            {
                site = GetSiteId(),
                status = UpdatedSiteStatus
            };
        }
        else
        {
            payload = new
            {
                site = GetSiteId(),
                status = (SiteStatus)20 // Invalid status
            };
        }

        Response = await GetHttpClientForTest().PostAsJsonAsync("http://localhost:7071/api/site-status", payload);
    }

    [Then(@"the call should fail with (\d*)")]
    public void AssertFailureCode(int statusCode) => Response.StatusCode.Should().Be((HttpStatusCode)statusCode);

    [Then("the default site should have an updated site status")]
    public async Task AssertUpdatedSiteStatus()
    {
        Response.EnsureSuccessStatusCode();

        var actualResult =
            await CosmosReadItem<SiteDocument>("core_data", GetSiteId(), new PartitionKey("site"), CancellationToken.None);
        
        actualResult.Resource.Status.Should().Be(UpdatedSiteStatus);
    }
}
