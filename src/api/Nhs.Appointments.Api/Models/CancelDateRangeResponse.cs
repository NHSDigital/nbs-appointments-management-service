namespace Nhs.Appointments.Api.Models;

public record CancelDateRangeResponse(int CancelledSessionsCount, int CancelledBookingsCount, int BookingsWithoutContactDetailsCount);
