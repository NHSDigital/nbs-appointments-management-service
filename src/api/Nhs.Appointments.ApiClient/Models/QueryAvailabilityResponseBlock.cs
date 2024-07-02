namespace Nhs.Appointments.ApiClient.Models
{
    public record QueryAvailabilityResponseBlock(TimeOnly from, TimeOnly until, int count);
}
