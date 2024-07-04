using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record TemplateAssignment(
        [property: JsonPropertyName("from")] string From,
        [property:JsonPropertyName("until")] string Until,
        [property:JsonPropertyName("templateId")] string TemplateId);

}
