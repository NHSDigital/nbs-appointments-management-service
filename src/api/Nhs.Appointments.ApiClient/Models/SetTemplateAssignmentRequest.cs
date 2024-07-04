using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record SetTemplateAssignmentRequest
    {
        [property: JsonPropertyName("site")]
        public string Site { get; set; }

        [property: JsonPropertyName("assignments")]
        public TemplateAssignment[] Assignments { get; set; }
    }
}
