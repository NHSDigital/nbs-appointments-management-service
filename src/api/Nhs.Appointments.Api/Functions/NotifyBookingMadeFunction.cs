using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Nhs.Appointments.Api.Consumers;
using Microsoft.AspNetCore.Authorization;

namespace Nhs.Appointments.Api.Functions;

public class NotifyBookingMadeFunction(IMessageReceiver receiver)
{
    public const string QueueName = "booking-made";

    [Function("NotifyBookingMade")]
    [AllowAnonymous]
    public Task NotifyBookingMadeAsync([ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        return receiver.HandleConsumer<BookingMadeConsumer>(QueueName, message, cancellationToken);
    }
}
