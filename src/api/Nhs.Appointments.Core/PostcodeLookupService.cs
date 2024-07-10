using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nhs.Appointments.Core;

public interface IPostcodeLookupService
{
    Task<(double latitude, double longitude)> GeolocationFromPostcode(string postcode);
}

public class PostcodeLookupService : IPostcodeLookupService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Options _options;

    public PostcodeLookupService(IHttpClientFactory httpClientFactory, IOptions<Options> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<(double latitude, double longitude)> GeolocationFromPostcode(string postcode)
    {
        var trimmedPostcode = postcode.Replace(" ", "");
        var query = System.Net.WebUtility.UrlEncode($"{trimmedPostcode}");

        using var httpClient = _httpClientFactory.CreateClient(_options.ServiceName);
        var pathAndQuery = $"service-search/postcodesandplaces?api-version=1&search={query}";
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, pathAndQuery);                        

        using var response = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var postcodeLookupResponse = JsonSerializer.Deserialize<PostcodeLookupSearchResponse>(responseContent);

        var mostRelevantEntry = postcodeLookupResponse?.Results.OrderByDescending(x => x.SearchScore)?.FirstOrDefault();

        if (mostRelevantEntry == null)
        {
            throw new KeyNotFoundException("Could not find an entry for the supplied postcode");
        }

        return (mostRelevantEntry.Latitude, mostRelevantEntry.Longitude);
    }

    public class Options
    {
        public string ServiceName { get; set; }            
    }

    internal class PostcodeLookupSearchResponse
    {
        [JsonPropertyName("value")]
        public List<PostcodeLookupSearchResult> Results { get; set; }
    }

    internal class PostcodeLookupSearchResult 
    {
        [JsonPropertyName("@search.score")]
        public double SearchScore { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
