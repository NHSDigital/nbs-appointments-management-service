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

namespace Nhs.Appointments.Api.Functions;
public class NotifyOktaUserRolesChangedFunction(
    IMessageReceiver receiver, 
    IFeatureToggleHelper featureToggleHelper,
    ILogger<NotifyOktaUserRolesChangedFunction> logger)
{
    public const string QueueName = "okta-user-roles-changed";

    [Function("NotifyOktaUserRolesChanged")]
    [AllowAnonymous]
    public async Task NotifyOktaUserRolesChangedAsync(
        [ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")] 
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken
    ){
        var isOktaEnabled = await featureToggleHelper.IsFeatureEnabled(Flags.OktaEnabled);

        if (!isOktaEnabled)
        {
            logger.LogError("Okta is disabled. Cannot process message from queue '{QueueName}'.", QueueName);

            await messageActions.DeadLetterMessageAsync(
                message,
                new Dictionary<string, object>(),
                 "OktaFeatureDisabled" ,
                 "Okta is disabled. Message cannot be processed.",
                cancellationToken
            );

            return;
        }
        
        await receiver.HandleConsumer<OktaUserRolesChangedConsumer>(QueueName, message, cancellationToken);
    }
}
