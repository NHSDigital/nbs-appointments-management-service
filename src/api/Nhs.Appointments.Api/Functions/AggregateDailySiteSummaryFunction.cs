using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Nhs.Appointments.Api.Consumers;
using Microsoft.AspNetCore.Authorization;

namespace Nhs.Appointments.Api.Functions;

public class AggregateDailySiteSummaryFunction(IMessageReceiver receiver)
{
    public const string QueueName = "aggregate-daily-site-summary";

    [Function("AggregateDailySiteSummary")]
    [AllowAnonymous]
    public Task NotifyUserRolesChangedAsync([ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        return receiver.HandleConsumer<UserRolesChangedConsumer>(QueueName, message, cancellationToken);
    }
}


