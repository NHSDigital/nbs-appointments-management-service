using Nhs.Appointments.ApiClient.Impl;

namespace Nhs.Appointments.Api.Client.Integration
{
    public class AuthTests : IntegrationTestBase
    {
        [Fact (Skip = "All tests in the Api.Client.Integration project are WIP and are not expected to pass.")]
        public async Task ApiShouldRejectBadSignature()
        {
            UseBadSigningKey();
            var exception = await Assert.ThrowsAsync<UnexpectedResponseException>(() => ApiClient.Sites.GetSitesForUser());
            Assert.Contains("401", exception.Message);
        }
    }
}