using MassTransit.Serialization;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class UserRolesChangedNotifierTests
{
    private UserRolesChangedNotifier _sut;
    private Mock<ISendEmails> _notificationClient = new ();
    private Mock<IRolesStore> _rolesStore = new ();
    private Mock<ISiteService> _siteService = new();
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
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site (Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId, It.Is<Dictionary<string, dynamic>>(dic => dic.ContainsKey("rolesAdded") && dic.ContainsKey("rolesRemoved")))).Verifiable();

        await _sut.Notify(Email, Site, ["newRole"], ["removedRole"]);
        _notificationClient.Verify();
    }

    [Fact]
    public async Task GetsNameOfSite()
    {
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0])))).Verifiable();

        await _sut.Notify(Email, Site, ["newRole"], ["removedRole"]);
        _siteService.Verify();
    }

    [Fact]
    public async Task GetsNamesOfRoles()
    {
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }])).Verifiable();
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        await _sut.Notify(Email, Site, ["newRole"], ["removedRole"]);
        _rolesStore.Verify();
    }

    [Fact]
    public async Task DoesNotSendNotificationIfNoChanges()
    {
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        await _sut.Notify(Email, Site, [], []);
        _notificationClient.Verify(x => 
            x.SendEmailAsync(Email, TemplateId, It.IsAny<Dictionary<string, dynamic>>()),
            Times.Never());
    }

    [Fact]
    public async Task UsesFriendlyNamesForRoles()
    {
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId, It.Is<Dictionary<string, dynamic>>(dic => GetValue(dic, "rolesAdded").Contains("New Role") && GetValue(dic, "rolesRemoved").Contains("Removed Role")))).Verifiable();

        await _sut.Notify(Email, Site, ["newRole"], ["removedRole"]);
        _notificationClient.Verify();
    }

    [Fact]
    public async Task GetsFriendlyRoleNameWhenIdContainsScopePrefixButDatabaseValuesDont()
    {
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId, It.Is<Dictionary<string, dynamic>>(dic => GetValue(dic, "rolesAdded").Contains("New Role") && GetValue(dic, "rolesRemoved").Contains("Removed Role")))).Verifiable();

        await _sut.Notify(Email, Site, ["canned:newRole"], ["canned:removedRole"]);
        _notificationClient.Verify();
    }

    private static string GetValue(Dictionary<string, dynamic> dic, string key)
    {
        return dic.TryGetValue(key, out string result) ? result : "";
    }
}
