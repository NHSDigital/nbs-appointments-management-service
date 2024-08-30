using MassTransit;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Consumers;

public class UserRolesChangedConsumer(IUserRolesChangedNotifier notifier) : IConsumer<UserRolesChanged>
{
    public Task Consume(ConsumeContext<UserRolesChanged> context)
    {
        return notifier.Notify(context.Message.User, context.Message.Site, context.Message.Added, context.Message.Removed);
    }
}

public class UserRolesChangedConsumerDefinition :
    ConsumerDefinition<UserRolesChangedConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<UserRolesChangedConsumer> consumerConfigurator)
    {
        consumerConfigurator.UseMessageRetry(x => x.Intervals(10, 100, 500, 1000));
    }
}
