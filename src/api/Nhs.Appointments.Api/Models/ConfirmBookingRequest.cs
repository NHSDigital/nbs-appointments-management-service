using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record ConfirmBookingRequestPayload(ContactItem[] contactDetails, string[] relatedBookings, string bookingToReschedule, string cancellationReason = null); 
public record ConfirmBookingRequest(
    string bookingReference,
    ContactItem[] contactDetails,
    string[] relatedBookings,
    string bookingToReschedule,
    string cancellationReason = null) : ConfirmBookingRequestPayload(contactDetails, relatedBookings, bookingToReschedule, cancellationReason);
