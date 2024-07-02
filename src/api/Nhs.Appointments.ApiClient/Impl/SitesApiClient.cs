using Microsoft.Extensions.Logging;
using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.ApiClient.Impl
{
    public class SitesApiClient : ApiClientBase, ISitesApiClient
    {
        public SitesApiClient(Func<HttpClient> httpClientFactory, ILogger logger) : base(httpClientFactory, logger)
        {
        }

        public async Task<IEnumerable<Site>> FindSitesByPostcode(string postcode)
        {
            var response = await Get<IEnumerable<Site>>($"api/sites?postcode={postcode}");
            return response;
        }

        public async Task<SiteConfiguration> GetSiteConfiguration(string site)
        {
            var response = await Get<SiteConfiguration>($"api/site-configuration?site={site}");
            return response;
        }

        public async Task<GetSiteMetaDataResponse> GetSiteMetaData(string site)
        {
            var response = await Get<GetSiteMetaDataResponse>($"api/site/meta?site={site}");
            return response;
        }

        public async Task<IEnumerable<Site>> GetSitesForUser()
        {
            var response = await Get<IEnumerable<Site>>("api/user/sites");
            return response;
        }

        public async Task SetSiteConfiguration(SiteConfiguration siteConfiguration)
        {
            await Post(siteConfiguration, "api/site-configuration");
        }
    }
}
