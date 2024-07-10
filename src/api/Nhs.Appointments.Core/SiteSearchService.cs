using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nhs.Appointments.Core;

public interface ISiteSearchService
{
    Task<IEnumerable<Site>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords);
    Task<Site> GetSiteByIdAsync(string siteId);
}

public record Site(string  Id, string Name, string Address);

public class SiteSearchService : ISiteSearchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Options _options;

    public SiteSearchService(IHttpClientFactory httpClientFactory, IOptions<Options> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<IEnumerable<Site>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords)
    {
        var searchRequest = new
        {
            filter = $"geo.distance(Geocode, geography'POINT({longitude} {latitude})') le {searchRadius}",
            orderby = $"geo.distance(Geocode, geography'POINT({longitude} {latitude})') asc",
            select = "UnitID, OrganisationName, Address, Latitude, Longitude",
            top = maximumRecords
        };
        using var httpClient = _httpClientFactory.CreateClient(_options.ServiceName);
        var response = await httpClient.PostAsJsonAsync("/covid-sites/search?api-version=1", searchRequest);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var siteSearchResponse = JsonSerializer.Deserialize<SiteSearchResponse>(responseContent);

        return siteSearchResponse.Sites.Select(s => new Site(s.UnitId.ToString(), s.SiteName, s.SiteAddress));
    }

    public async Task<Site> GetSiteByIdAsync(string siteId)
    {
        var searchRequest = new
        {
            search= $"{siteId}",
            select = "UnitID, OrganisationName, Address, Latitude, Longitude",
            top = 1
        };
        using var httpClient = _httpClientFactory.CreateClient(_options.ServiceName);
        var response = await httpClient.PostAsJsonAsync("/covid-sites/search?api-version=1", searchRequest);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var siteSearchResponse = JsonSerializer.Deserialize<SiteSearchResponse>(responseContent);
        var match = siteSearchResponse.Sites.SingleOrDefault(s => s.UnitId.ToString() == siteId);
        return match != null ? new Site(match.UnitId.ToString(), match.SiteName, match.SiteAddress) : null;
    }

    public class Options
    {
        public string ServiceName { get; set; }
    }

    internal class SiteSearchResponse
    {
        [JsonPropertyName("value")]
        public List<SiteSearchResponseEntry> Sites { get; set; }
    }

    internal class SiteSearchResponseEntry
    {
        [JsonPropertyName("UnitID")]
        public int UnitId { get; set; }

        [JsonPropertyName("OrganisationName")]
        public string SiteName { get; set; }

        [JsonPropertyName("Address")]
        public string SiteAddress { get; set; }
    }
}
