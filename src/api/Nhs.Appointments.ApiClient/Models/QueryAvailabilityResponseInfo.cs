namespace Nhs.Appointments.ApiClient.Models
{
    public record QueryAvailabilityResponseInfo(DateOnly date, IEnumerable<QueryAvailabilityResponseBlock> blocks);
}
