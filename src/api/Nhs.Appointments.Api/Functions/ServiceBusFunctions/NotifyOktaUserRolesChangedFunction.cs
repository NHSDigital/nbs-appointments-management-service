using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Core.Features;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Functions.ServiceBusFunctions;
public class NotifyOktaUserRolesChangedFunction(IMessageReceiver receiver)
{
    public const string QueueName = "okta-user-roles-changed";

    [Function("NotifyOktaUserRolesChanged")]
    [AllowAnonymous]
    public async Task NotifyOktaUserRolesChangedAsync(
        [ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")] 
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken
    )
    {
        await receiver.HandleConsumer<OktaUserRolesChangedConsumer>(QueueName, message, cancellationToken);
    }
}
