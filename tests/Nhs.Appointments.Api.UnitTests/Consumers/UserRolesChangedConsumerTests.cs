using MassTransit;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Events;
using Nhs.Appointments.Api.Notifications;

namespace Nhs.Appointments.Api.Tests.Consumers
{
    public class UserRolesChangedConsumerTests
    {
        private UserRolesChangedConsumer _sut;
        private readonly Mock<IUserRolesChangedNotifier> _notifier = new();
        public UserRolesChangedConsumerTests()
        {
            _sut = new UserRolesChangedConsumer(_notifier.Object);
        }

        [Fact]
        public async Task NotifiesUserOnEventReceipt()
        {
            const string user = "test@tempuri.org";
            string[] roles = ["role1"];
            _notifier.Setup(x => x.Notify(user, It.Is<string[]>(r => Enumerable.SequenceEqual(r, roles)))).Verifiable();
            var ctx = new Mock<ConsumeContext<UserRolesChanged>>();
            ctx.SetupGet(x => x.Message).Returns(new UserRolesChanged { User = user, Roles = roles });
            await _sut.Consume(ctx.Object);
            _notifier.Verify();
        }
    }
}
