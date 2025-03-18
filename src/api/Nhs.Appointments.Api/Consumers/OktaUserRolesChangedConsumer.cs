using MassTransit;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Consumers;
public class OktaUserRolesChangedConsumer(IUserRolesChangedNotifier notifier) : IConsumer<OktaUserRolesChanged>
{
    public Task Consume(ConsumeContext<OktaUserRolesChanged> context)
    {
        return notifier.Notify(nameof(OktaUserRolesChanged), context.Message.UserId, context.Message.SiteId, context.Message.AddedRoleIds, context.Message.RemovedRoleIds);
    }
}
