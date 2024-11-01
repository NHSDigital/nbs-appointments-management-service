namespace Nhs.Appointments.Api.Models;

public record ConfirmBookingRequestPayload(ContactItem[] contactDetails); 
public record ConfirmBookingRequest(string bookingReference, ContactItem[] contactDetails) : ConfirmBookingRequestPayload(contactDetails);
