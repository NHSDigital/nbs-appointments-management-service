using Moq;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class BookingMadeNotifierTests
{
    private BookingMadeNotifier _sut;
    const string EmailTemplateId = "email-template";
    const string SmsTemplateId = "sms-template";
    const string Site = "site:some-site";
    const string Email = "test@tempuri.org";
    const string FirstName = "joe";
    const string PhoneNumber = "0123456789";
    const bool EmailContactConsent = true;
    const bool PhoneContactConsent = true;
    const string Reference = "booking-ref-1234";
    const string Service = "some-service";
    DateOnly date = new DateOnly(2050, 1, 1);
    TimeOnly time = new TimeOnly(12, 15);

    private Mock<ISendNotifications> _notificationClient = new();
    private Mock<ISiteSearchService> _siteService = new();
    private Mock<INotificationConfigurationStore> _notificationConfigurationStore = new();

    public BookingMadeNotifierTests()
    {
       _sut = new BookingMadeNotifier(_notificationClient.Object, _notificationConfigurationStore.Object, _siteService.Object);
    }

    [Fact]
    public async Task PassesValuesToGovNotifyService()
    {
        _notificationConfigurationStore.Setup(x => x.GetNotificationConfigurationForService(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId }));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street")));

        _notificationClient.Setup(x => x.SendEmailAsync(Email, EmailTemplateId, It.Is<Dictionary<string, dynamic>>(
            dic => 
            dic.ContainsKey("firstName") &&
            dic.ContainsKey("siteName") &&
            dic.ContainsKey("date") &&
            dic.ContainsKey("time") &&
            dic.ContainsKey("address") &&
            dic.ContainsKey("reference")
            ))).Verifiable();

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, EmailContactConsent, Email, PhoneContactConsent, PhoneNumber);
        _notificationClient.Verify();
    }


    [Fact]
    public async Task GetsNameOfSite()
    {
        _notificationConfigurationStore.Setup(x => x.GetNotificationConfigurationForService(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId }));
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street"))).Verifiable();
        _notificationClient.Setup(x => x.SendEmailAsync(Email, EmailTemplateId, It.IsAny<Dictionary<string, dynamic>>()));

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, EmailContactConsent, Email, PhoneContactConsent, PhoneNumber);
        _siteService.Verify();
    }

    [Fact]
    public async Task GetsCorrectNotificationConfiguration()
    {
        _notificationConfigurationStore.Setup(x => x.GetNotificationConfigurationForService(Service, "BookingMade")).Returns(Task.FromResult(new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId })).Verifiable();
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>())).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street")));
        _notificationClient.Setup(x => x.SendEmailAsync(Email, EmailTemplateId, It.IsAny<Dictionary<string, dynamic>>()));

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, EmailContactConsent, Email, PhoneContactConsent, PhoneNumber);
        _notificationConfigurationStore.Verify();
    }
}
