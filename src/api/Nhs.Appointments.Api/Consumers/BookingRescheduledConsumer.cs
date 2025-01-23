using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Consumers;

public class BookingRescheduledConsumer(IBookingNotifier bookingNotifier) : BookingConsumer<BookingRescheduled>(bookingNotifier)
{
    
}
