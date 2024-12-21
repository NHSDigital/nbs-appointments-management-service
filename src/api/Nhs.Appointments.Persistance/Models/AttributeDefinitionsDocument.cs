using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class AttributeDefinitionsDocument : CoreDataCosmosDocument
{
    [JsonProperty("attributeDefinitions")]
    public IEnumerable<AttributeDefinition> AttributeDefinitions { get; set; }
}
