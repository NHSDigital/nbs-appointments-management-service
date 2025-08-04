using MassTransit.Serialization;
using Moq;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class UserRolesChangedNotifierTests
{
    private const string TemplateId = "my-template";
    private const string Email = "test@user";
    private const string Site = "6877d86e-c2df-4def-8508-e1eccf0ea6ba";
    private readonly Mock<ISendNotifications> _notificationClient = new();
    private readonly Mock<INotificationConfigurationService> _notificationConfigurationService = new();
    private readonly Mock<IRolesStore> _rolesStore = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly UserRolesChangedNotifier _sut;

    public UserRolesChangedNotifierTests()
    {
        _sut = new UserRolesChangedNotifier(_notificationClient.Object, _notificationConfigurationService.Object,
            _rolesStore.Object, _siteService.Object);
    }

    [Fact]
    public async Task PassesValuesToGovNotifyService()
    {
        _notificationConfigurationService.Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>()))
            .ReturnsAsync(new NotificationConfiguration { EmailTemplateId = TemplateId });
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([
            new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }
        ]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>()))
            .Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N",
                "R1", "ICB1", "Information For Citizens 123", Array.Empty<Accessibility>(), new Location("point", [0, 0]), SiteStatus.Online)));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId,
            It.Is<Dictionary<string, dynamic>>(dic =>
                dic.ContainsKey("rolesAdded") && dic.ContainsKey("rolesRemoved")))).Verifiable();

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["newRole"], ["removedRole"]);
        _notificationClient.Verify();
    }

    [Fact]
    public async Task GetsNameOfSite()
    {
        _notificationConfigurationService.Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>()))
            .ReturnsAsync(new NotificationConfiguration { EmailTemplateId = TemplateId });
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([
            new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }
        ]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>())).Returns(
            Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N", "R1", "ICB1", "Information For Citizens 123",
                Array.Empty<Accessibility>(), new Location("point", [0, 0]), SiteStatus.Online))).Verifiable();

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["newRole"], ["removedRole"]);
        _siteService.Verify();
    }

    [Fact]
    public async Task GetsNamesOfRoles()
    {
        _notificationConfigurationService.Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>()))
            .ReturnsAsync(new NotificationConfiguration { EmailTemplateId = TemplateId });
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([
            new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }
        ])).Verifiable();
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>()))
            .Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N",
                "R1", "ICB1", "Information For Citizens 123", Array.Empty<Accessibility>(), new Location("point", [0, 0]), SiteStatus.Online)));

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["newRole"], ["removedRole"]);
        _rolesStore.Verify();
    }

    [Fact]
    public async Task DoesNotSendNotificationIfNoChanges()
    {
        _notificationConfigurationService.Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>()))
            .ReturnsAsync(new NotificationConfiguration { EmailTemplateId = TemplateId });
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([
            new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }
        ]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>()))
            .Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N",
                "R1", "ICB1", "Information For Citizens 123", Array.Empty<Accessibility>(), new Location("point", [0, 0]), SiteStatus.Online)));

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, [], []);
        _notificationClient.Verify(x =>
                x.SendEmailAsync(Email, TemplateId, It.IsAny<Dictionary<string, dynamic>>()),
            Times.Never());
    }

    [Fact]
    public async Task UsesFriendlyNamesForRoles()
    {
        _notificationConfigurationService.Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>()))
            .ReturnsAsync(new NotificationConfiguration { EmailTemplateId = TemplateId });
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([
            new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }
        ]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>()))
            .Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N",
                "R1", "ICB1", "Information For Citizens 123", Array.Empty<Accessibility>(), new Location("point", [0, 0]), SiteStatus.Online)));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId,
            It.Is<Dictionary<string, dynamic>>(dic =>
                GetValue(dic, "rolesAdded").Contains("New Role") &&
                GetValue(dic, "rolesRemoved").Contains("Removed Role")))).Verifiable();

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["newRole"], ["removedRole"]);
        _notificationClient.Verify();
    }

    [Fact]
    public async Task GetsFriendlyRoleNameWhenIdContainsScopePrefixButDatabaseValuesDont()
    {
        _notificationConfigurationService.Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>()))
            .ReturnsAsync(new NotificationConfiguration { EmailTemplateId = TemplateId });
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([
            new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }
        ]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>()))
            .Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N",
                "R1", "ICB1", "Information For Citizens 123", Array.Empty<Accessibility>(), new Location("point", [0, 0]), SiteStatus.Online)));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId,
            It.Is<Dictionary<string, dynamic>>(dic =>
                GetValue(dic, "rolesAdded").Contains("New Role") &&
                GetValue(dic, "rolesRemoved").Contains("Removed Role")))).Verifiable();

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["canned:newRole"], ["canned:removedRole"]);
        _notificationClient.Verify();
    }


    [Fact]
    public async Task GetsCorrectNotificationConfiguration()
    {
        _notificationConfigurationService.Setup(x => x.GetNotificationConfigurationsAsync("UserRolesChanged"))
            .ReturnsAsync(new NotificationConfiguration { EmailTemplateId = TemplateId }).Verifiable();
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([
            new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }
        ]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>()))
            .Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N",
                "R1", "ICB1", "Information For Citizens 123", Array.Empty<Accessibility>(), new Location("point", [0, 0]), SiteStatus.Online)));

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["newRole"], ["removedRole"]);
        _notificationConfigurationService.Verify();
    }

    private static string GetValue(Dictionary<string, dynamic> dic, string key)
    {
        return dic.TryGetValue(key, out string result) ? result : "";
    }
}
