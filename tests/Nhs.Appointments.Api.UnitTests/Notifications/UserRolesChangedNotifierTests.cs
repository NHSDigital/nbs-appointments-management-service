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
    private Mock<ISiteSearchService> _siteService = new();
    private const string TemplateId = "my-template";
    private const string Email = "test@user";
    private const string Site = "site1";
    public UserRolesChangedNotifierTests()
    {

        var opts = new Mock<IOptions<UserRolesChangedNotifier.Options>>();
        opts.SetupGet(x => x.Value).Returns(new UserRolesChangedNotifier.Options { EmailTemplateId = TemplateId});
        _sut = new UserRolesChangedNotifier(_notificationClient.Object, _rolesStore.Object, _siteService.Object, opts.Object);
    }

    [Fact]
    public async Task PassesValuesToGovNotifyService()
    {
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site (Site, "A Clinical Site", "123 Surgery Street")));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId, It.Is<Dictionary<string, dynamic>>(dic => dic.ContainsKey("rolesAdded") && dic.ContainsKey("rolesRemoved")), null, null, null)).Verifiable();

        await _sut.Notify(Email, Site, ["newRole"], ["removedRole"]);
        _notificationClient.Verify();
    }

    [Fact]
    public async Task GetsNameOfSite()
    {
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street"))).Verifiable();

        await _sut.Notify(Email, Site, ["newRole"], ["removedRole"]);
        _siteService.Verify();
    }

    [Fact]
    public async Task GetsNamesOfRoles()
    {
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }])).Verifiable();
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street")));

        await _sut.Notify(Email, Site, ["newRole"], ["removedRole"]);
        _rolesStore.Verify();
    }
}
