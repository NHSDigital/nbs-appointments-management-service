using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record SetBookingStatusRequest(string bookingReference, AppointmentStatus status);
public record SetBookingStatusResponse(string bookingReference, AppointmentStatus status);
