using MassTransit;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Consumers;

public class BookingCancelledConsumer(IBookingCancelledNotifier notifier, TimeProvider time) : IConsumer<BookingCancelled>
{
    public Task Consume(ConsumeContext<BookingCancelled> context)
    {
        if(context.Message.ContactDetails == null)
        {
            return Task.CompletedTask;
        }

        if (BookingIsInThePast(context.Message))
        {
            return Task.CompletedTask;
        }

        var email = context.Message.ContactDetails.FirstOrDefault(x => x.Type == Core.ContactItemType.Email)?.Value;
        var phone = context.Message.ContactDetails.FirstOrDefault(x => x.Type == Core.ContactItemType.Phone)?.Value;

        return notifier.Notify(
            nameof(BookingCancelled),
            context.Message.Service,
            context.Message.Reference,
            context.Message.Site,
            context.Message.FirstName,
            DateOnly.FromDateTime(context.Message.From),
            TimeOnly.FromDateTime(context.Message.From),
            email,
            phone
            );
    }

    private bool BookingIsInThePast(BookingCancelled message)
    {
        return message.From < time.GetUtcNow();
    }
}
