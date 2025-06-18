using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using System.Threading;
using Nhs.Appointments.Api.Consumers;
using Microsoft.AspNetCore.Authorization;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Api.Functions;
public class NotifyOktaUserRolesChangedFunction(IMessageReceiver receiver, IFeatureToggleHelper featureToggleHelper)
{
    public const string QueueName = "okta-user-roles-changed";

    [Function("NotifyOktaUserRolesChanged")]
    [AllowAnonymous]
    public async Task NotifyOktaUserRolesChangedAsync(
        [ServiceBusTrigger(QueueName, Connection = "ServiceBusConnectionString")] 
        ServiceBusReceivedMessage message, 
        CancellationToken cancellationToken
    ){
        var isOktaEnabled = await featureToggleHelper.IsFeatureEnabled(Flags.OktaEnabled);

        if (isOktaEnabled)
        {
            await receiver.HandleConsumer<OktaUserRolesChangedConsumer>(QueueName, message, cancellationToken);
        }
    }
}
