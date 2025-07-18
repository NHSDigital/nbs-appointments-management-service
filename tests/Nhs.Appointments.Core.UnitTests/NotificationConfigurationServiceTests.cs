using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core.UnitTests;
public class NotificationConfigurationServiceTests
{
    private readonly Mock<IMemoryCache> _memoryCacheMock = new();
    private readonly Mock<INotificationConfigurationStore> _storeMock = new();
    private readonly NotificationConfigurationService _sut;
    private static readonly string[] Covid5_11Services = { "COVID:5_11" };

    public NotificationConfigurationServiceTests()
    {
        _sut = new NotificationConfigurationService(_memoryCacheMock.Object, _storeMock.Object);
    }

    [Fact]
    public async Task GetNotificationConfigurationsAsync_WithMatchingEventTypeAndService_ReturnsCombinedConfiguration()
    {
        // Arrange
        var eventType = "AppointmentBooked";
        var service = "COVID:5_11";

        var configs = new List<NotificationConfiguration>
        {
            new NotificationConfiguration
            {
                EventType = "AppointmentBooked",
                Services = Covid5_11Services,
                EmailTemplateId = "email-template-1",
                SmsTemplateId = null
            },
            new NotificationConfiguration
            {
                EventType = "AppointmentBooked",
                Services = Covid5_11Services,
                EmailTemplateId = null,
                SmsTemplateId = "sms-template-2"
            }
        };

        SetupMemoryCache(configs);

        // Act
        var result = await _sut.GetNotificationConfigurationsAsync(eventType, service);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventType, result.EventType);
        Assert.Single(result.Services);
        Assert.Equal(service, result.Services.First());
        Assert.Equal("email-template-1", result.EmailTemplateId);
        Assert.Equal("sms-template-2", result.SmsTemplateId);
    }

    [Fact]
    public async Task GetNotificationConfigurationsAsync_NoMatchingService_ReturnsNull()
    {
        // Arrange
        var eventType = "AppointmentBooked";
        var service = "UnknownService";

        var configs = new List<NotificationConfiguration>
        {
            new NotificationConfiguration
            {
                EventType = "AppointmentBooked",
                Services = Covid5_11Services,
                EmailTemplateId = "email-template-1",
                SmsTemplateId = "sms-template-1"
            }
        };

        SetupMemoryCache(configs);

        // Act
        var result = await _sut.GetNotificationConfigurationsAsync(eventType, service);

        // Assert
        Assert.Null(result);
    }

    private void SetupMemoryCache(IEnumerable<NotificationConfiguration> cacheEntry)
    {
        _memoryCacheMock
            .Setup(mc => mc.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
            .Callback(new TryGetValueCallback((object key, out object value) =>
            {
                value = cacheEntry;
            }))
            .Returns(true);
    }

    private delegate void TryGetValueCallback(object key, out object value);
}
