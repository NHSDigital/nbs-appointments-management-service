using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("site")]
public class SiteDocument : CoreDataCosmosDocument
{
    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("address")] public string Address { get; set; }

    [JsonProperty("phoneNumber")] public string PhoneNumber { get; set; }

    [JsonProperty("odsCode")] public string OdsCode { get; set; }

    [JsonProperty("region")] public string Region { get; set; }

    [JsonProperty("integratedCareBoard")] public string IntegratedCareBoard { get; set; }

    [JsonProperty("location")] public Location Location { get; set; }

    [JsonProperty("informationForCitizens")] public string InformationForCitizens { get; set; }

    [JsonProperty("accessibilities")] public Accessibility[] Accessibilities { get; set; }

    [JsonProperty("referenceNumberGroup")] public int ReferenceNumberGroup { get; set; }

    [JsonProperty("type")] public string Type { get; set; }
}
