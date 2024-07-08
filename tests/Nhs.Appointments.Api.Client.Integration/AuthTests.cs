using Nhs.Appointments.ApiClient.Impl;

namespace Nhs.Appointments.Api.Client.Integration
{
    public class AuthTests : IntegrationTestBase
    {
        [Fact]
        public async Task ApiShouldRejectBadSignature()
        {
            UseBadSigningKey();
            var exception = await Assert.ThrowsAsync<UnexpectedResponseException>(() => ApiClient.Sites.GetSitesForUser());
            Assert.Contains("401", exception.Message);
        }
    }
}