using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class AccessibilityDefinitionsDocument : CoreDataCosmosDocument
{
    [JsonProperty("AccessibilityDefinitions")]
    public IEnumerable<AccessibilityDefinition> AccessibilityDefinitions { get; set; }
}
