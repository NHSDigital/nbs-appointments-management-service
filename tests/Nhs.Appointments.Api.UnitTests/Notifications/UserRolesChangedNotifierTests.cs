using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;
using Notify.Interfaces;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class UserRolesChangedNotifierTests
{
    private UserRolesChangedNotifier _sut;
    private Mock<IAsyncNotificationClient> _notificationClient = new ();
    private Mock<IRolesStore> _rolesStore = new ();
    private const string TemplateId = "my-template";
    private const string Email = "test@user";
    public UserRolesChangedNotifierTests()
    {

        var opts = new Mock<IOptions<UserRolesChangedNotifier.Options>>();
        opts.SetupGet(x => x.Value).Returns(new UserRolesChangedNotifier.Options { EmailTemplateId = TemplateId});
        _sut = new UserRolesChangedNotifier(_notificationClient.Object, _rolesStore.Object, opts.Object);
    }

    [Fact]
    public async Task PassesValuesToGovNotifyService()
    {
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId, It.Is<Dictionary<string, dynamic>>(dic => dic.ContainsKey("rolesAdded") && dic.ContainsKey("rolesRemoved")), null, null, null)).Verifiable();

        await _sut.Notify(Email, ["newRole"], ["removedRole"]);
        _notificationClient.Verify();
    }
}
