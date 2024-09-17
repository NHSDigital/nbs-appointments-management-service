using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Nhs.Appointments.Api.Consumers;
using Microsoft.AspNetCore.Authorization;

namespace Nhs.Appointments.Api.Functions;

public class NotifyUserRolesChangedFunction(IMessageReceiver receiver)
{
    const string UserRolesChangedQueueName = "user-roles-changed";

    [Function("NotifyUserRolesChanged")]
    [AllowAnonymous]
    public Task NotifyUserRolesChangedAsync([ServiceBusTrigger(UserRolesChangedQueueName, Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        return receiver.HandleConsumer<UserRolesChangedConsumer>(UserRolesChangedQueueName, message, cancellationToken);
    }
}


