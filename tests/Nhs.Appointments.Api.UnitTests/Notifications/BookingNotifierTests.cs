using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Api.Tests.Notifications;

public class BookingNotifierTests
{
    private const string EmailTemplateId = "email-template";
    private const string SmsTemplateId = "sms-template";
    private const string Site = "6877d86e-c2df-4def-8508-e1eccf0ea6ba";
    private const string Email = "test@tempuri.org";
    private const string FirstName = "joe";
    private const string PhoneNumber = "07722333444";
    private const string Reference = "booking-ref-1234";
    private const string Service = "some-service";
    private readonly Mock<ILogger<BookingNotifier>> _logger = new();

    private readonly Mock<ISendNotifications> _notificationClient = new();
    private readonly Mock<INotificationConfigurationService> _notificationConfigurationService = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IClinicalServiceProvider> _clinicalServiceProviderMock = new();
    private readonly BookingNotifier _sut;
    private readonly DateOnly date = new(2050, 1, 1);
    private readonly TimeOnly time = new(12, 15);

    public BookingNotifierTests()
    {
        _sut = new BookingNotifier(_notificationClient.Object, _notificationConfigurationService.Object,
            _siteService.Object, new PrivacyUtil(), _logger.Object, _clinicalServiceProviderMock.Object);
    }

    [Fact]
    public async Task PassesValuesToEmailGovNotifyService()
    {
        var clinicalService = new ClinicalServiceType()
        {
            ServiceType = "COVID-19",
            Url = "https://www.nhs.uk/bookcovid"
        };

        _notificationConfigurationService
            .Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
                new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>()))
            .Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N",
                "R1", "ICB1", "Information For Citizens 123", null, null, SiteStatus.Online)));
        _clinicalServiceProviderMock.Setup(x => x.Get(Service)).ReturnsAsync(clinicalService);
        _notificationClient.Setup(x => x.SendEmailAsync(Email, EmailTemplateId, It.Is<Dictionary<string, dynamic>>(
            dic =>
                dic.ContainsKey("firstName") &&
                dic.ContainsKey("siteName") &&
                dic.ContainsKey("date") &&
                dic.ContainsKey("time") &&
                dic.ContainsKey("address") &&
                dic.ContainsKey("reference") &&
                dic.ContainsKey("siteLocation") &&
                dic.ContainsKey("vaccine") &&
                dic.ContainsKey("serviceURL") 
        ))).Verifiable();

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, NotificationType.Email,
            Email);
        _notificationClient.Verify();
    }

    [Fact]
    public async Task PassesValuesToSmsGovNotifyService()
    {
        var clinicalService = new ClinicalServiceType() { 
            ServiceType = "COVID-19" , 
            Url = "https://www.nhs.uk/bookcovid" 
        };

        _notificationConfigurationService
            .Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
                new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>()))
            .Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N",
                "R1", "ICB1", "Information For Citizens 123", null, null, SiteStatus.Online)));
        _clinicalServiceProviderMock.Setup(x => x.Get(Service)).ReturnsAsync(clinicalService);
        _notificationClient.Setup(x => x.SendSmsAsync(PhoneNumber, SmsTemplateId, It.Is<Dictionary<string, dynamic>>(
            dic =>
                dic.ContainsKey("firstName") &&
                dic.ContainsKey("siteName") &&
                dic.ContainsKey("date") &&
                dic.ContainsKey("time") &&
                dic.ContainsKey("address") &&
                dic.ContainsKey("reference") &&
                dic.ContainsKey("siteLocation") &&
                dic.ContainsKey("vaccine") &&
                dic.ContainsKey("serviceURL")
        )));

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, NotificationType.Sms,
            PhoneNumber);

        _notificationClient.Verify(x => x.SendSmsAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.Is<Dictionary<string, dynamic>>(dic =>
                dic.ContainsKey("vaccine") &&
                (dic.GetValueOrDefault("vaccine") as string).Contains(clinicalService.ServiceType) &&
                dic.ContainsKey("serviceURL") &&
                (dic.GetValueOrDefault("serviceURL") as string).Contains(clinicalService.Url)
            )
        ), Times.Once);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("Information For Citizens 123", true)]
    [InlineData(null, false)]
    public async Task SiteLocationIsRenderedCorrectly(string informationForCitizens, bool hasPrefixText)
    {
        var clinicalService = new ClinicalServiceType()
        {
            ServiceType = "COVID-19",
            Url = "https://www.nhs.uk/bookcovid"
        };
        var prefixText = "Additional site information:";

        _notificationConfigurationService
            .Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
                new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>()))
            .Returns(Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N",
                "R1", "ICB1", informationForCitizens, null, null, SiteStatus.Online)));
        _clinicalServiceProviderMock.Setup(x => x.Get(Service)).ReturnsAsync(clinicalService);
        _notificationClient.Setup(x => x.SendEmailAsync(Email, EmailTemplateId, It.Is<Dictionary<string, dynamic>>(
            dic =>
                dic.ContainsKey("firstName") &&
                dic.ContainsKey("siteName") &&
                dic.ContainsKey("date") &&
                dic.ContainsKey("time") &&
                dic.ContainsKey("address") &&
                dic.ContainsKey("reference") &&
                dic.ContainsKey("siteLocation")
        )));

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, NotificationType.Email, Email);

        _notificationClient.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.Is<Dictionary<string, dynamic>>(dic =>
                dic.ContainsKey("siteLocation") &&                     
                (dic.GetValueOrDefault("siteLocation") as string).Contains(hasPrefixText ? prefixText : "")
            )
        ), Times.Once); 
    }


    [Fact]
    public async Task GetsNameOfSite()
    {
        var clinicalService = new ClinicalServiceType()
        {
            ServiceType = "COVID-19",
            Url = "https://www.nhs.uk/bookcovid"
        };
        _notificationConfigurationService
            .Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
                new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.Is<string>(s => s == Site), It.IsAny<string>())).Returns(
            Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N", "R1", "ICB1", "Information For Citizens 123",
                Array.Empty<Accessibility>(), new Location("point", [0, 0]), SiteStatus.Online))).Verifiable();
        _clinicalServiceProviderMock.Setup(x => x.Get(Service)).ReturnsAsync(clinicalService);
        _notificationClient.Setup(
            x => x.SendEmailAsync(Email, EmailTemplateId, It.IsAny<Dictionary<string, dynamic>>()));

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, NotificationType.Email,
            Email);
        _siteService.Verify();
    }

    [Fact]
    public async Task GetsCorrectNotificationConfiguration()
    {
        var clinicalService = new ClinicalServiceType()
        {
            ServiceType = "COVID-19",
            Url = "https://www.nhs.uk/bookcovid"
        };

        _notificationConfigurationService
            .Setup(x => x.GetNotificationConfigurationsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(
                new NotificationConfiguration { EmailTemplateId = EmailTemplateId, SmsTemplateId = SmsTemplateId });
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
            Task.FromResult(new Site(Site, "A Clinical Site", "123 Surgery Street", "0113 1111111", "15N", "R1", "ICB1", "Information For Citizens 123",
                Array.Empty<Accessibility>(), new Location("point", [0, 0]), SiteStatus.Online)));
        _clinicalServiceProviderMock.Setup(x => x.Get(Service)).ReturnsAsync(clinicalService);
        _notificationClient.Setup(
            x => x.SendEmailAsync(Email, EmailTemplateId, It.IsAny<Dictionary<string, dynamic>>()));

        await _sut.Notify(nameof(BookingMade), Service, Reference, Site, FirstName, date, time, NotificationType.Email,
            Email);
        _notificationConfigurationService.Verify();
    }
}
