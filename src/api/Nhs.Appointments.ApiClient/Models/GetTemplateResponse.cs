namespace Nhs.Appointments.ApiClient.Models
{
    public record GetTemplateResponse
    {
        public WeekTemplate[] Templates { get; set; }
    }
}
