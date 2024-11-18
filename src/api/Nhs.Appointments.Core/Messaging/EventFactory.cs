using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core.Messaging;

public interface IBookingEventFactory
{
    BookingMade BuildBookingMadeEvent(Booking booking);
    BookingCancelled BuildBookingCancelledEvent(Booking booking);
    BookingRescheduled BuildBookingRescheduledEvent(Booking booking);
    BookingReminder BuildBookingReminderEvent(Booking booking);
}
public class EventFactory : IBookingEventFactory
{
    public BookingRescheduled BuildBookingRescheduledEvent(Booking booking)
    {
        if (booking.ContactDetails == null)
        {
            throw new ArgumentException("The booking must include contact details", nameof(booking.ContactDetails));
        }

        return new BookingRescheduled
        {
            FirstName = booking.AttendeeDetails.FirstName,
            From = booking.From,
            LastName = booking.AttendeeDetails.LastName,
            Reference = booking.Reference,
            Service = booking.Service,
            Site = booking.Site,
            ContactDetails = booking.ContactDetails.Select(c => new Messaging.Events.ContactItem { Type = c.Type, Value = c.Value }).ToArray()
        };
    }

    public BookingMade BuildBookingMadeEvent(Booking booking)
    {
        if (booking.ContactDetails == null)
        {
            throw new ArgumentException("The booking must include contact details", nameof(booking.ContactDetails));
        }

        return new BookingMade
        {
            FirstName = booking.AttendeeDetails.FirstName,
            From = booking.From,
            LastName = booking.AttendeeDetails.LastName,
            Reference = booking.Reference,
            Service = booking.Service,
            Site = booking.Site,
            ContactDetails = booking.ContactDetails.Select(c => new Messaging.Events.ContactItem { Type = c.Type, Value = c.Value }).ToArray()
        };
    }

    public BookingCancelled BuildBookingCancelledEvent(Booking booking)
    {
        return new BookingCancelled
        {
            FirstName = booking.AttendeeDetails?.FirstName,
            From = booking.From,
            LastName = booking.AttendeeDetails?.LastName,
            Reference = booking.Reference,
            Service = booking.Service,
            Site = booking.Site,
            ContactDetails = booking.ContactDetails?.Select(c => new Messaging.Events.ContactItem { Type = c.Type, Value = c.Value }).ToArray()
        };
    }

    public BookingReminder BuildBookingReminderEvent(Booking booking)
    {
        if (booking.ContactDetails == null)
        {
            throw new ArgumentException("The booking must include contact details", nameof(booking.ContactDetails));
        }

        return new BookingReminder
        {
            FirstName = booking.AttendeeDetails.FirstName,
            From = booking.From,
            LastName = booking.AttendeeDetails.LastName,
            Reference = booking.Reference,
            Service = booking.Service,
            Site = booking.Site,
            ContactDetails = booking.ContactDetails.Select(c => new Messaging.Events.ContactItem { Type = c.Type, Value = c.Value }).ToArray()
        };
    }
}
