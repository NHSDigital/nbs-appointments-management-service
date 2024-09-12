using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class AttributeSetsDocument : IndexDataCosmosDocument
{
    [JsonProperty("sets")]
    public AttributeSet[] Sets { get; set; }
}