using FluentAssertions;
using Gherkin.Ast;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Integration.Collections;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core.Features;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Nhs.Appointments.Api.Integration.Scenarios.SiteManagement
{
    [FeatureFile("./Scenarios/SiteManagement/GetSiteMetaData.feature")]
    public abstract class GetSiteMetaDataFeatureSteps(string flag, bool enabled) : SiteManagementBaseFeatureSteps(flag, enabled)
    {
        [When("I request site meta data for site '(.+)'")]
        public async Task RequestSiteMetaData(string site)
        {
            var siteId = GetSiteId(site);
            Response = await Http.GetAsync($"http://localhost:7071/api/sites/{siteId}/meta");
        }

        [Then("the correct site meta data is returned")]
        public async Task Assert(DataTable dataTable)
        {
            var row = dataTable.Rows.ElementAt(1);

            var expectedSite = new GetSiteMetaDataResponse(
                Site: row.Cells.ElementAt(0).Value,
                AdditionalInformation: row.Cells.ElementAt(1).Value);

            Response.StatusCode.Should().Be(HttpStatusCode.OK);
            var (_, actualResponse) =
                await JsonRequestReader.ReadRequestAsync<GetSiteMetaDataResponse>(
                    await Response.Content.ReadAsStreamAsync());
            actualResponse.Should().BeEquivalentTo(expectedSite);
        }

        [Then("no site meta data is returned")]
        public async Task AssertEmpty()
        {
            Response.StatusCode.Should().Be(HttpStatusCode.OK);
            var (_, actualResponse) =
                await JsonRequestReader.ReadRequestAsync<GetSiteMetaDataResponse>(
                    await Response.Content.ReadAsStreamAsync());
            actualResponse.AdditionalInformation.Should().BeEmpty();
        }
    }

    [Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
    [FeatureFile("./Scenarios/SiteManagement/GetSiteMetaData.feature")]
    public class GetSiteMetaDataFeaturesSteps_LastUpdatedByEnabled() : GetSiteMetaDataFeatureSteps(Flags.AuditLastUpdatedBy, true);


    [Collection(FeatureToggleCollectionNames.LastUpdatedByCollection)]
    [FeatureFile("./Scenarios/SiteManagement/GetSiteMetaData.feature")]
    public class GetSiteMetaDataFeatureSteps_LastUpdatedByDisabled() : GetSiteMetaDataFeatureSteps(Flags.AuditLastUpdatedBy, false);
}
