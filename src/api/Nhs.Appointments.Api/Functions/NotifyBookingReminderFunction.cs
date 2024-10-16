using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Nhs.Appointments.Api.Consumers;
using Microsoft.AspNetCore.Authorization;

namespace Nhs.Appointments.Api.Functions;

public class NotifyBookingReminderFunction(IMessageReceiver receiver)
{
    public const string QueueName = "booking-reminder";

    [Function("NotifyBookingReminder")]
    [AllowAnonymous]
    public Task NotifyBookingReminderAsync([ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        return receiver.HandleConsumer<BookingReminderConsumer>(QueueName, message, cancellationToken);
    }
}


