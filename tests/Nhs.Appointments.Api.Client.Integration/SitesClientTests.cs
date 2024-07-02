namespace Nhs.Appointments.Api.Client.Integration
{
    public class SitesClientTests : IntegrationTestBase
    {
        [Fact]
        public async Task CanListSites()
        {
            var sites = await ApiClient.Sites.GetSitesForUser();
            Assert.NotNull(sites);
            Assert.True(sites.Count() > 0);
        }
    }
}