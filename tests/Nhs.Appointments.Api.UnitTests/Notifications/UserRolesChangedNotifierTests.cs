﻿using MassTransit.Serialization;
using Moq;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Messaging.Events;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography.Xml;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class UserRolesChangedNotifierTests
{
    private UserRolesChangedNotifier _sut;
    private Mock<ISendNotifications> _notificationClient = new ();
    private Mock<IRolesStore> _rolesStore = new ();
    private Mock<ISiteService> _siteService = new();
    private Mock<INotificationConfigurationStore> _notificationConfigurationStore = new ();
    private const string TemplateId = "my-template";
    private const string Email = "test@user";
    private const string Site = "site1";
    public UserRolesChangedNotifierTests()
    {
        _sut = new UserRolesChangedNotifier(_notificationClient.Object, _notificationConfigurationStore.Object, _rolesStore.Object,  _siteService.Object);
    }

    [Fact]
    public async Task PassesValuesToGovNotifyService()
    {
        _notificationConfigurationStore.Setup(x => x.GetNotificationConfiguration(It.IsAny<string>())).Returns(Task.FromResult(new NotificationConfiguration { EmailTemplateId = TemplateId }));
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site (Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId, It.Is<Dictionary<string, dynamic>>(dic => dic.ContainsKey("rolesAdded") && dic.ContainsKey("rolesRemoved")))).Verifiable();

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["newRole"], ["removedRole"]);
        _notificationClient.Verify();
    }

    [Fact]
    public async Task GetsNameOfSite()
    {
        _notificationConfigurationStore.Setup(x => x.GetNotificationConfiguration(It.IsAny<string>())).Returns(Task.FromResult(new NotificationConfiguration { EmailTemplateId = TemplateId }));
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0])))).Verifiable();

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["newRole"], ["removedRole"]);
        _siteService.Verify();
    }

    [Fact]
    public async Task GetsNamesOfRoles()
    {
        _notificationConfigurationStore.Setup(x => x.GetNotificationConfiguration(It.IsAny<string>())).Returns(Task.FromResult(new NotificationConfiguration { EmailTemplateId = TemplateId }));
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }])).Verifiable();
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["newRole"], ["removedRole"]);
        _rolesStore.Verify();
    }

    [Fact]
    public async Task DoesNotSendNotificationIfNoChanges()
    {
        _notificationConfigurationStore.Setup(x => x.GetNotificationConfiguration(It.IsAny<string>())).Returns(Task.FromResult(new NotificationConfiguration { EmailTemplateId = TemplateId }));
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, [], []);
        _notificationClient.Verify(x => 
            x.SendEmailAsync(Email, TemplateId, It.IsAny<Dictionary<string, dynamic>>()),
            Times.Never());
    }

    [Fact]
    public async Task UsesFriendlyNamesForRoles()
    {
        _notificationConfigurationStore.Setup(x => x.GetNotificationConfiguration(It.IsAny<string>())).Returns(Task.FromResult(new NotificationConfiguration { EmailTemplateId = TemplateId }));
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId, It.Is<Dictionary<string, dynamic>>(dic => GetValue(dic, "rolesAdded").Contains("New Role") && GetValue(dic, "rolesRemoved").Contains("Removed Role")))).Verifiable();

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["newRole"], ["removedRole"]);
        _notificationClient.Verify();
    }

    [Fact]
    public async Task GetsFriendlyRoleNameWhenIdContainsScopePrefixButDatabaseValuesDont()
    {
        _notificationConfigurationStore.Setup(x => x.GetNotificationConfiguration(It.IsAny<string>())).Returns(Task.FromResult(new NotificationConfiguration { EmailTemplateId = TemplateId }));
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, TemplateId, It.Is<Dictionary<string, dynamic>>(dic => GetValue(dic, "rolesAdded").Contains("New Role") && GetValue(dic, "rolesRemoved").Contains("Removed Role")))).Verifiable();

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["canned:newRole"], ["canned:removedRole"]);
        _notificationClient.Verify();
    }


    [Fact]
    public async Task GetsCorrectNotificationConfiguration()
    {
        _notificationConfigurationStore.Setup(x => x.GetNotificationConfiguration("UserRolesChanged")).Returns(Task.FromResult(new NotificationConfiguration { EmailTemplateId = TemplateId })).Verifiable();
        _rolesStore.Setup(x => x.GetRoles()).Returns(Task.FromResult<IEnumerable<Role>>([new Role { Id = "newRole", Name = "New Role" }, new Role { Id = "removedRole", Name = "Removed Role" }]));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));

        await _sut.Notify(nameof(UserRolesChanged), Email, Site, ["newRole"], ["removedRole"]);
        _notificationConfigurationStore.Verify();
    }

    private static string GetValue(Dictionary<string, dynamic> dic, string key)
    {
        return dic.TryGetValue(key, out string result) ? result : "";
    }
}
