using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Nhs.Appointments.Api.Consumers;
using Microsoft.AspNetCore.Authorization;

namespace Nhs.Appointments.Api.Functions;

public class NotifyBookingCancelledFunction(IMessageReceiver receiver)
{
    public const string QueueName = "booking-cancelled";

    [Function("NotifyBookingCancelled")]
    [AllowAnonymous]
    public Task NotifyBookingCancelledAsync([ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        return receiver.HandleConsumer<BookingCancelledConsumer>(QueueName, message, cancellationToken);
    }
}
