using MassTransit;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Consumers;

public class BookingMadeConsumer(IBookingMadeNotifier notifier) : IConsumer<BookingMade>
{
    public Task Consume(ConsumeContext<BookingMade> context)
    {
        var email = context.Message.ContactDetails.FirstOrDefault(x => x.Type == "email")?.Value;
        var phone = context.Message.ContactDetails.FirstOrDefault(x => x.Type == "phone")?.Value;

        return notifier.Notify(
            nameof(BookingMade),
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
}
