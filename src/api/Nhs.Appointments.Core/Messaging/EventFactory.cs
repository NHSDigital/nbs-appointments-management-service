using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core.Messaging;

public interface IBookingEventFactory
{
    T[] BuildBookingEvents<T>(Booking booking) where T : PatientBookingNotificationEventBase, new();
}
public class EventFactory : IBookingEventFactory
{
    public T[] BuildBookingEvents<T>(Booking booking) where T : PatientBookingNotificationEventBase, new()
    {
        if (booking.ContactDetails == null)
        {
            throw new ArgumentException("The booking must include contact details", nameof(booking.ContactDetails));
        }

        var result = new List<T>();

        if (booking.ContactDetails.Any(x => x.Type == ContactItemType.Phone))
        {
            result.Add(BuildEvent<T>(booking, NotificationType.Sms));
        }

        if (booking.ContactDetails.Any(x => x.Type == ContactItemType.Email))
        {
            result.Add(BuildEvent<T>(booking, NotificationType.Email));
        }

        return result.ToArray();
    }

    private static T BuildEvent<T>(Booking booking, NotificationType notificationType) where T : PatientBookingNotificationEventBase, new()
    {
        return new T
        {
            NotificationType = notificationType,
            FirstName = booking.AttendeeDetails?.FirstName,
            From = booking.From,
            LastName = booking.AttendeeDetails?.LastName,
            Reference = booking.Reference,
            Service = booking.Service,
            Site = booking.Site,
            ContactDetails = booking.ContactDetails.ToArray()
        };
    }
}
