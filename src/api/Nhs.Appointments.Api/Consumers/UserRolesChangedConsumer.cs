using MassTransit;
using Nhs.Appointments.Api.Events;
using Nhs.Appointments.Api.Notifications;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Consumers
{
    public class UserRolesChangedConsumer : IConsumer<UserRolesChanged>
    {
        private readonly IUserRolesChangedNotifier _notifier;

        public UserRolesChangedConsumer(IUserRolesChangedNotifier notifier)
        {
            _notifier = notifier;
        }

        public Task Consume(ConsumeContext<UserRolesChanged> context)
        {
            return _notifier.Notify(context.Message.User, context.Message.Roles);

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
}
