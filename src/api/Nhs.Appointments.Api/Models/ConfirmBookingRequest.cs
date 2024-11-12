namespace Nhs.Appointments.Api.Models;

public record ConfirmBookingRequestPayload(ContactItem[] contactDetails, string bookingToReschedule); 
public record ConfirmBookingRequest(string bookingReference, ContactItem[] contactDetails, string bookingToReschedule) : ConfirmBookingRequestPayload(contactDetails, bookingToReschedule);
