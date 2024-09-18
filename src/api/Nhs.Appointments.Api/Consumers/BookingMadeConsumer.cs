using MassTransit;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using System;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Consumers;

public class BookingMadeConsumer(IBookingMadeNotifier notifier) : IConsumer<BookingMade>
{
    public Task Consume(ConsumeContext<BookingMade> context)
    {
        return notifier.Notify(
            nameof(BookingMade),
            context.Message.Service,
            context.Message.Reference,
            context.Message.Site,
            context.Message.FirstName,
            DateOnly.FromDateTime(context.Message.From),
            TimeOnly.FromDateTime(context.Message.From),
            context.Message.EmailContactConsent,
            context.Message.Email,
            context.Message.PhoneContactConsent,
            context.Message.PhoneNumber
            );
    }
}
