namespace Nhs.Appointments.Api.Models;

public record SetBookingStatusRequest(string bookingReference, string status);
public record SetBookingStatusResponse(string bookingReference, string status);
