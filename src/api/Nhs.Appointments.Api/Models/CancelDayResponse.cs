namespace Nhs.Appointments.Api.Models;
public record CancelDayResponse(int CancelledBookingCount, int BookingsWithoutContactDetails);
