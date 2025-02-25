using Newtonsoft.Json;

namespace CapacityDataExtracts.Documents;
internal class SiteSessionParquet
{
    [JsonProperty("DATE")]
    public string Date { get; set; }

    [JsonProperty("TIME")]
    public string Time { get; set; }

    [JsonProperty("SLOT_LENGTH")]
    public string SlotLength { get; set; }

    [JsonProperty("CAPACITY")]
    public int Capacity { get; set; }

    [JsonProperty("ODS_CODE")]
    public string OdsCode { get; set; }

    [JsonProperty("SITE_NAME")]
    public string SiteName { get; set; }

    [JsonProperty("REGION")]
    public string Region { get; set; }

    [JsonProperty("LATITUDE")]
    public double Latitude { get; set; }

    [JsonProperty("LONGITUDE")]
    public double Longitude { get; set; }

    [JsonProperty("ICB")]
    public string IntegratedCareBoard { get; set; }

    [JsonProperty("SERVICE")]
    public string[] Service { get; set; }
}
