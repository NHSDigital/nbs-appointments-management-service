using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.WebJobs;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Nhs.Appointments.Api.Consumers;

namespace Nhs.Appointments.Api.Functions;

public class NotifyUserRolesChangedFunction
{
    const string UserRolesChangedQueueName = "user-role-change";
    readonly IMessageReceiver _receiver;

    public NotifyUserRolesChangedFunction(IMessageReceiver receiver)
    {
        _receiver = receiver;
    }

    [FunctionName("NotifyUserRolesChanged")]
    public Task NotifyUserRolesChangedAsync([ServiceBusTrigger(UserRolesChangedQueueName)] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        return _receiver.HandleConsumer<UserRolesChangedConsumer>(UserRolesChangedQueueName, message, cancellationToken);
    }
}


