using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Nhs.Appointments.Api.Consumers;
using Microsoft.AspNetCore.Authorization;

namespace Nhs.Appointments.Api.Functions;

public class NotifyBookingRescheduledFunction(IMessageReceiver receiver)
{
    public const string QueueName = "booking-rescheduled";

    [Function("NotifyBookingRescheduled")]
    [AllowAnonymous]
    public Task NotifyBookingRescheduledAsync([ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        return receiver.HandleConsumer<BookingRescheduledConsumer>(QueueName, message, cancellationToken);
    }
}
