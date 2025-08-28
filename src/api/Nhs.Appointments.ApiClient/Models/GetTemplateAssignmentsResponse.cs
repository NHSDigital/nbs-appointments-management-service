using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record GetTemplateAssignmentsResponse([property: JsonPropertyName("assignments")] TemplateAssignment[] Assignments);
}
