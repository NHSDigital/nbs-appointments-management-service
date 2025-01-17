using MassTransit;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using System;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Consumers;

public abstract class BookingConsumer<TNotification>(IBookingNotifier notifier) : IConsumer<TNotification> where TNotification : PatientBookingNotificationEventBase
{
    public Task Consume(ConsumeContext<TNotification> context)
    {
        if (NotificationIsValid(context.Message) == false)
        {
            return Task.CompletedTask;
        }

        if (context.Message.NotificationType == NotificationType.Unknown)
            throw new NotSupportedException("Invalid notification type");

        return notifier.Notify(
            typeof(TNotification).Name,
            context.Message.Service,
            context.Message.Reference,
            context.Message.Site,
            context.Message.FirstName,
            DateOnly.FromDateTime(context.Message.From),
            TimeOnly.FromDateTime(context.Message.From),
            context.Message.NotificationType,
            context.Message.Destination);
    }

    protected virtual bool NotificationIsValid(TNotification notification) => true;
}
