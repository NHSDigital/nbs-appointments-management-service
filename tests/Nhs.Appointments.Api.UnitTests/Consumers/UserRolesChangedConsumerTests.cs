using MassTransit;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;

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
            const string site = "site1";
            string[] rolesAdded = ["role1"];
            string[] rolesRemoved = ["role2"];
            _notifier.Setup(x => x.Notify(user, site, It.Is<string[]>(r => Enumerable.SequenceEqual(r, rolesAdded)), It.Is<string[]>(r => Enumerable.SequenceEqual(r, rolesRemoved)))).Verifiable();
            var ctx = new Mock<ConsumeContext<UserRolesChanged>>();
            ctx.SetupGet(x => x.Message).Returns(new UserRolesChanged { UserId = user, SiteId = site, AddedRoleIds = rolesAdded, RemovedRoleIds = rolesRemoved });

            await _sut.Consume(ctx.Object);

            _notifier.Verify();
        }
    }
}
