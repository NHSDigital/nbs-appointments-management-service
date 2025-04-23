using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class ClinicalServiceDocument : CoreDataCosmosDocument
{
    [JsonProperty("services")]
    public ClinicalServiceTypeDocument[] Services { get; set; }
}

public class ClinicalServiceTypeDocument 
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("label")]
    public string Label { get; set; }
}
