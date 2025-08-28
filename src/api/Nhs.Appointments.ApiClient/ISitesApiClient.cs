using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.ApiClient
{
    public interface ISitesApiClient
    {
        Task<SiteConfiguration> GetSiteConfiguration(string site);
        Task<GetSiteMetaDataResponse> GetSiteMetaData(string site);
        Task<IEnumerable<Site>> GetSitesForUser();
        Task SetSiteConfiguration(SiteConfiguration siteConfiguration);
    }
}
