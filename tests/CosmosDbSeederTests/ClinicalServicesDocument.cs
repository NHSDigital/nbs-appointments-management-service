using System.Text.Json.Serialization;

namespace CosmosDbSeederTests;

public class ClinicalServicesDocument
{
    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("docType")] public required string DocType { get; set; }

    [JsonPropertyName("services")] public required Service[] Services { get; set; }
}

public class Service
{
    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("label")] public required string Label { get; set; }
}
