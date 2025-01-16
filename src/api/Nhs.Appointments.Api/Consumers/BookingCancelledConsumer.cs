using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using System;

namespace Nhs.Appointments.Api.Consumers;

public class BookingCancelledConsumer(IBookingNotifier bookingNotifier, TimeProvider timeProvider) : BookingConsumer<BookingCancelled>(bookingNotifier)
{
    protected override bool NotificationIsValid(BookingCancelled notification) => notification.From > timeProvider.GetUtcNow();
}
