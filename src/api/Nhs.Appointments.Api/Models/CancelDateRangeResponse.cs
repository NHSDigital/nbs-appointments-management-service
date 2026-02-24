namespace Nhs.Appointments.Api.Models;

public record CancelDateRangeResponse(int CancelledSessionCount, int CancelledBookingsCount, int BookingsWithoutContactDetailsCount);
