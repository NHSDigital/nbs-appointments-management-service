using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class AttributeDefinitionsDocument : IndexDataCosmosDocument
{
    [JsonProperty("attributeDefinitions")]
    public IEnumerable<AttributeDefinition> AttributeDefinitions { get; set; }
}