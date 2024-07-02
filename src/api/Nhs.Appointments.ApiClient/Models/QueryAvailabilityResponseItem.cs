namespace Nhs.Appointments.ApiClient.Models
{
    public record QueryAvailabilityResponseItem(string site, string service, List<QueryAvailabilityResponseInfo> availability);
}
