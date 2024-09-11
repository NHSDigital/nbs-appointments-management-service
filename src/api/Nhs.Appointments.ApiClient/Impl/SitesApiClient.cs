using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.ApiClient.Configuration;
using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.ApiClient.Impl
{
    public class SitesApiClient : ApiClientBase, ISitesApiClient
    {
        public SitesApiClient([FromKeyedServices(ContainerConfiguration.HttpClientKey)] Func<HttpClient> httpClientFactory, ILogger<SitesApiClient> logger) : base(httpClientFactory, logger)
        {
        }
        public Task<SiteConfiguration> GetSiteConfiguration(string site) => Get<SiteConfiguration>($"api/site-configuration?site={site}");

        public Task<GetSiteMetaDataResponse> GetSiteMetaData(string site) => Get<GetSiteMetaDataResponse>($"api/site/meta?site={site}");

        public Task<IEnumerable<Site>> GetSitesForUser() => Get<IEnumerable<Site>>("api/user/sites");

        public Task SetSiteConfiguration(SiteConfiguration siteConfiguration) => Post("api/site-configuration", siteConfiguration);
    }
}
