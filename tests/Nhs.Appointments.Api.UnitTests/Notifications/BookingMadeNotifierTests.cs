using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;

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
    DateOnly date = new DateOnly(2050, 1, 1);
    TimeOnly time = new TimeOnly(12, 15);

    private Mock<ISendNotifications> _notificationClient = new();
    private Mock<ISiteSearchService> _siteService = new();

    public BookingMadeNotifierTests()
    {
        var opts = new Mock<IOptions<BookingMadeNotifier.Options>>();
        opts.SetupGet(x => x.Value).Returns(new BookingMadeNotifier.Options { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId });
        _sut = new BookingMadeNotifier(_notificationClient.Object, _siteService.Object, opts.Object);
    }

    [Fact]
    public async Task PassesValuesToGovNotifyService()
    {
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

        await _sut.Notify(Reference, Site, FirstName, date, time, EmailContactConsent, Email, PhoneContactConsent, PhoneNumber);
        _notificationClient.Verify();
    }


    [Fact]
    public async Task GetsNameOfSite()
    {
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site))).Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street"))).Verifiable();

        _notificationClient.Setup(x => x.SendEmailAsync(Email, EmailTemplateId, It.IsAny<Dictionary<string, dynamic>>()));

        await _sut.Notify(Reference, Site, FirstName, date, time, EmailContactConsent, Email, PhoneContactConsent, PhoneNumber);
        _siteService.Verify();
    }
}
