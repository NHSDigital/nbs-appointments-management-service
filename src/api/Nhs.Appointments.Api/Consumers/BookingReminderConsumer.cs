using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Consumers;

public class BookingReminderConsumer(IBookingNotifier bookingNotifier) : BookingConsumer<BookingReminder>(bookingNotifier)
{
    
}
