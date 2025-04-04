using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Nhs.Appointments.Api.Consumers;
using Microsoft.AspNetCore.Authorization;

namespace Nhs.Appointments.Api.Functions;
public class NotifyOktaUserRolesChangedFunction(IMessageReceiver receiver)
{
    public const string QueueName = "okta-user-roles-changed";

    [Function("NotifyOktaUserRolesChanged")]
    [AllowAnonymous]
    public Task NotifyOktaUserRolesChangedAsync([ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        return receiver.HandleConsumer<OktaUserRolesChangedConsumer>(QueueName, message, cancellationToken);
    }
}
