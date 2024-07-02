using Newtonsoft.Json;

namespace Nhs.Appointments.ApiClient.Models
{
    public record TemplateAssignment(
        [JsonProperty("from")] string From,
        [JsonProperty("until")] string Until,
        [JsonProperty("templateId")] string TemplateId);

}
