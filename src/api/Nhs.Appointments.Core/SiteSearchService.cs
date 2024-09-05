using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Nhs.Appointments.Core;

public interface ISiteSearchService
{
    Task<IEnumerable<Site>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords);
    Task<Site> GetSiteByIdAsync(string siteId);
}

public class SiteSearchService(ISiteStore siteStore, IHttpClientFactory httpClientFactory, IOptions<SiteSearchService.Options> options) : ISiteSearchService
{
    private readonly Options _options = options.Value;

    public async Task<IEnumerable<Site>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords)
    {
        var sites = await siteStore.GetSitesByArea(longitude, latitude, searchRadius);
        return sites;
    }


    public async Task<Site> GetSiteByIdAsync(string siteId)
    {
        var searchRequest = new
        {
            search= $"{siteId}",
            select = "UnitID, OrganisationName, Address, Latitude, Longitude",
            top = 1
        };
        using var httpClient = httpClientFactory.CreateClient(_options.ServiceName);
        var response = await httpClient.PostAsJsonAsync("/covid-sites/search?api-version=1", searchRequest);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var siteSearchResponse = JsonConvert.DeserializeObject<SiteSearchResponse>(responseContent);
        var match = siteSearchResponse.Sites.SingleOrDefault(s => s.UnitId.ToString() == siteId);
        return match != null ? new Site(match.UnitId.ToString(), match.SiteName, match.SiteAddress) : null;
    }

    public class Options
    {
        public string ServiceName { get; set; }
    }

    internal class SiteSearchResponse
    {
        [JsonProperty("value")]
        public List<SiteSearchResponseEntry> Sites { get; set; }
    }

    internal class SiteSearchResponseEntry
    {
        [JsonProperty("UnitID")]
        public int UnitId { get; set; }

        [JsonProperty("OrganisationName")]
        public string SiteName { get; set; }

        [JsonProperty("Address")]
        public string SiteAddress { get; set; }
    }
}
