using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using System;

namespace Nhs.Appointments.Api.Consumers;
public class BookingAutoCancelledConsumer(IBookingNotifier bookingNotifier, TimeProvider timeProvider) : BookingConsumer<BookingAutoCancelled>(bookingNotifier)
{
    protected override bool NotificationIsValid(BookingAutoCancelled notification) => notification.From > timeProvider.GetUtcNow();
}
