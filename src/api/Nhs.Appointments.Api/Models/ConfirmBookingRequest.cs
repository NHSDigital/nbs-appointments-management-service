using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record ConfirmBookingRequestPayload(ContactItem[] contactDetails, string[] relatedBookings, string bookingToReschedule); 
public record ConfirmBookingRequest(string bookingReference, ContactItem[] contactDetails, string[] relatedBookings, string bookingToReschedule) : ConfirmBookingRequestPayload(contactDetails, relatedBookings, bookingToReschedule);
