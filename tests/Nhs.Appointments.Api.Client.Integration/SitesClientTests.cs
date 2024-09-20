namespace Nhs.Appointments.Api.Client.Integration
{
    public class SitesClientTests : IntegrationTestBase
    {
        [Fact (Skip = "All tests in the Api.Client.Integration project are WIP and are not expected to pass.")]
        public async Task CanListSites()
        {
            var sites = await ApiClient.Sites.GetSitesForUser();
            Assert.NotNull(sites);
            Assert.True(sites.Count() > 0);
        }
    }
}