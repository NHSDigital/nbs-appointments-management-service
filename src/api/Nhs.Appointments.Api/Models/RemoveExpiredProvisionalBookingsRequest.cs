namespace Nhs.Appointments.Api.Models;
public record RemoveExpiredProvisionalBookingsRequest(
    int? BatchSize,
    int? DegreeOfParallelism);
