using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("week_template")]
public class WeekTemplateDocument : BookingDataCosmosDocument
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("items")]
    public Schedule[] TemplateItems { get; set; }
}

[CosmosDocumentType("template_assignments")]
public class TemplateAssignmentDocument : BookingDataCosmosDocument
{
    public TemplateAssignment[] Assignments { get; set; }
}