using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record GetTemplateResponse
    {
        [property: JsonPropertyName("templates")]
        public WeekTemplate[] Templates { get; set; }
    }
}
