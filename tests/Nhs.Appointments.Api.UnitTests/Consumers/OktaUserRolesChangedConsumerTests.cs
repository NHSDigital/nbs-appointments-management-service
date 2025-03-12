using MassTransit;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Tests.Consumers;
public class OktaUserRolesChangedConsumerTests
{
    private OktaUserRolesChangedConsumer _sut;
    private readonly Mock<IUserRolesChangedNotifier> _notifier = new();
    public OktaUserRolesChangedConsumerTests()
    {
        _sut = new OktaUserRolesChangedConsumer(_notifier.Object);
    }

    [Fact]
    public async Task NotifiesUserOnEventReceipt()
    {
        const string user = "test@tempuri.org";
        const string site = "site1";
        string[] rolesAdded = ["role1"];
        string[] rolesRemoved = ["role2"];
        _notifier.Setup(x => x.Notify(nameof(OktaUserRolesChanged), user, site, It.Is<string[]>(r => Enumerable.SequenceEqual(r, rolesAdded)), It.Is<string[]>(r => Enumerable.SequenceEqual(r, rolesRemoved)))).Verifiable();
        var ctx = new Mock<ConsumeContext<OktaUserRolesChanged>>();
        ctx.SetupGet(x => x.Message).Returns(new OktaUserRolesChanged { UserId = user, SiteId = site, AddedRoleIds = rolesAdded, RemovedRoleIds = rolesRemoved });

        await _sut.Consume(ctx.Object);

        _notifier.Verify();
    }
}
