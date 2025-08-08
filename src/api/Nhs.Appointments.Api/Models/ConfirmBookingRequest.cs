using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record ConfirmBookingRequestPayload(ContactItem[] contactDetails, string[] relatedBookings, string bookingToReschedule, CancellationReason? cancellationReason); 
public record ConfirmBookingRequest(
    string bookingReference,
    ContactItem[] contactDetails,
    string[] relatedBookings,
    string bookingToReschedule,
    CancellationReason? cancellationReason) : ConfirmBookingRequestPayload(contactDetails, relatedBookings, bookingToReschedule, cancellationReason);
