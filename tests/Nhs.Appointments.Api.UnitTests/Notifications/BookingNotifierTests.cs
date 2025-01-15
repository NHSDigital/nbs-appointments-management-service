using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class BookingNotifierTests
{
    private BookingNotifier _sut;
    const string EmailTemplateId = "email-template";
    const string SmsTemplateId = "sms-template";
    const string Site = "site:some-site";
    const string Email = "test@tempuri.org";
    const string FirstName = "joe";
    const string PhoneNumber = "0123456789";
    const string Reference = "booking-ref-1234";
    const string Service = "some-service";
    DateOnly date = new (2050, 1, 1);
    TimeOnly time = new (12, 15);

    private Mock<ISendNotifications> _notificationClient = new();
    private Mock<ISiteService> _siteService = new();
    private Mock<INotificationConfigurationService> _notificationConfigurationService = new();
    private Mock<ILogger> _logger = new();

    public BookingNotifierTests()
    {
       _sut = new BookingNotifier(_notificationClient.Object, _notificationConfigurationService.Object, _siteService.Object, new PrivacyUtil(), _logger.Object);
    }

    [Fact]
    public async Task PassesValuesToGovNotifyService()
    {
        _notificationConfigurationService.Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>()))
            .Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "R1", "ICB1",null, null)));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, EmailTemplateId, It.Is<Dictionary<string, dynamic>>(
            dic => 
            dic.ContainsKey("firstName") &&
            dic.ContainsKey("siteName") &&
            dic.ContainsKey("date") &&
            dic.ContainsKey("time") &&
            dic.ContainsKey("address") &&
            dic.ContainsKey("reference")
            ))).Verifiable();

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, NotificationType.Email, Email);
        _notificationClient.Verify();
    }


    [Fact]
    public async Task GetsNameOfSite()
    {
        _notificationConfigurationService.Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>())).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "R1", "ICB1",Array.Empty<AttributeValue>(), new Location("point", [0, 0])))).Verifiable();
        _notificationClient.Setup(x => x.SendEmailAsync(Email, EmailTemplateId, It.IsAny<Dictionary<string, dynamic>>()));

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, NotificationType.Email, Email);
        _siteService.Verify();
    }

    [Fact]
    public async Task GetsCorrectNotificationConfiguration()
    {
        _notificationConfigurationService.Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "R1", "ICB1",Array.Empty<AttributeValue>(), new Location("point", [0, 0]))));
        _notificationClient.Setup(x => x.SendEmailAsync(Email, EmailTemplateId, It.IsAny<Dictionary<string, dynamic>>()));

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, NotificationType.Email, Email);
        _notificationConfigurationService.Verify();
    }
}
