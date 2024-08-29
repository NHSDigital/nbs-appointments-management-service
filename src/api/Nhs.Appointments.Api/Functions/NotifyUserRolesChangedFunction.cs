using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.WebJobs;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Nhs.Appointments.Api.Consumers;

namespace Nhs.Appointments.Api.Functions;

public class NotifyUserRolesChangedFunction(IMessageReceiver receiver)
{
    const string UserRolesChangedQueueName = "user-role-change";

    [FunctionName("NotifyUserRolesChanged")]
    public Task NotifyUserRolesChangedAsync([ServiceBusTrigger(UserRolesChangedQueueName)] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        return receiver.HandleConsumer<UserRolesChangedConsumer>(UserRolesChangedQueueName, message, cancellationToken);
    }
}


