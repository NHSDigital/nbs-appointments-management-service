namespace Nhs.Appointments.Api.Integration
{
    public class SitesTests :  IClassFixture<AzureFunctionsTestcontainersFixture>
    {
        private readonly AzureFunctionsTestcontainersFixture _azureFunctionsTestcontainersFixture;

        public SitesTests(AzureFunctionsTestcontainersFixture azureFunctionsTestcontainersFixture)
        {
            _azureFunctionsTestcontainersFixture = azureFunctionsTestcontainersFixture;
        }
        
        [Fact]
        public async Task CanListSites()
        {
            var response = await _azureFunctionsTestcontainersFixture.ApiClient.Sites.GetSitesForUser();
            Assert.NotNull(response);
            Assert.True(response.Any());
        }
    }
}
