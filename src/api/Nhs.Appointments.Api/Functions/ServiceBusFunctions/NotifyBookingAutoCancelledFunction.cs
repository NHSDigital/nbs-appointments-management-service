using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Nhs.Appointments.Api.Consumers;
using System.Threading;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions.ServiceBusFunctions;
public class NotifyBookingAutoCancelledFunction(IMessageReceiver receiver)
{
    public const string QueueName = "booking-auto-cancellation";

    [AllowAnonymous]
    [Function("NotifyBookingAutoCancelled")]
    public Task NotifyBookingAutoCancelledAsync([ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        return receiver.HandleConsumer<BookingAutoCancelledConsumer>(QueueName, message, cancellationToken);
    }
}
