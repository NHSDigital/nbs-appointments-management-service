using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record ConfirmBookingRequestPayload(ContactItem[] contactDetails, string[] childBookings, string bookingToReschedule); 
public record ConfirmBookingRequest(string bookingReference, ContactItem[] contactDetails, string[]? childBookings, string bookingToReschedule) : ConfirmBookingRequestPayload(contactDetails, childBookings, bookingToReschedule);
