namespace Nhs.Appointments.ApiClient.Models
{
    public record QueryBookingsRequest(DateTime from, DateTime to, string site);
}
