using Newtonsoft.Json;

namespace Nhs.Appointments.ApiClient.Models
{
    public record SetTemplateAssignmentRequest
    {
        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("assignments")]
        public TemplateAssignment[] Assignments { get; set; }
    }
}
