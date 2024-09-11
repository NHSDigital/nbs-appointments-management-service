using MassTransit;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Consumers;

public class UserRolesChangedConsumer(IUserRolesChangedNotifier notifier) : IConsumer<UserRolesChanged>
{
    public Task Consume(ConsumeContext<UserRolesChanged> context)
    {
        return notifier.Notify(context.Message.UserId, context.Message.SiteId, context.Message.AddedRoleIds, context.Message.RemovedRoleIds);
    }
}
