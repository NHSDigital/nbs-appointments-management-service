using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Api.Notifications.Options;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Reports.SiteSummary;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Api.Tests;

public class NotificationsServiceProviderExtensionsTests
{
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    [Fact(DisplayName = "Registers the null message bus if provider is none")]
    public void NotificationsRegistration_Null()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Notifications_Provider", "none" },
                { "GovNotifyBaseUri", "https://api.notifications.service.gov.uk" },
                { "GovNotifyApiKey", "some-api-key-123abc" }
            })
            .Build();

        var serviceProvider = _serviceCollection
            .AddLogging()
            .AddUserNotifications(configuration)
            .BuildServiceProvider();

        var messageBus = serviceProvider.GetService(typeof(IMessageBus));

        messageBus.Should().BeOfType<NullMessageBus>();
    }

    [Fact(DisplayName = "Registers the local message bus and cosmos notification client if provider is local")]
    public void NotificationsRegistration_Local()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Notifications_Provider", "local" },
                { "GovNotifyBaseUri", "https://api.notifications.service.gov.uk" },
                { "GovNotifyApiKey", "some-api-key-123abc" }
            })
            .Build();

        var serviceProvider = _serviceCollection
            .AddDependenciesNotUnderTest()
            .AddUserNotifications(configuration)
            .AddCosmosDataStores()
            .AddSingleton<ISiteSummaryAggregator, FakeSiteSummaryAggregator>()
            .AddSingleton<IClinicalServiceStore, ClinicalServiceStore>()
            .AddTransient<IClinicalServiceProvider, ClinicalServiceProvider>()
            .BuildServiceProvider();

        var messageBus = serviceProvider.GetService(typeof(IMessageBus));
        messageBus.Should().BeOfType<ConsoleLogWithMessageDelivery>();

        var notificationsClient = serviceProvider.GetService(typeof(ISendNotifications));
        notificationsClient.Should().BeOfType<CosmosNotificationClient>();
    }

    [Fact(DisplayName = "Registers the fake notification client if provider is azure-throttled")]
    public void NotificationsRegistration_AzureThrottled()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Notifications_Provider", "azure-throttled" },
                { "GovNotifyBaseUri", "https://api.notifications.service.gov.uk" },
                { "GovNotifyApiKey", "some-api-key-123abc" }
            })
            .Build();

        var serviceProvider = _serviceCollection
            .AddDependenciesNotUnderTest()
            .AddUserNotifications(configuration)
            .BuildServiceProvider();

        var notificationsClient = serviceProvider.GetService(typeof(ISendNotifications));
        notificationsClient.Should().BeOfType<FakeNotificationClient>();
    }

    [Fact(DisplayName = "Registers the real notification client if provider is azure")]
    public void NotificationsRegistration_Azure()
    {
        // Dummy strings throw a format error when requesting this service, so I've
        // generated then IMMEDIATELY REVOKED this Api Key on one of our test accounts.
        var revokedApiKey = "tempkeytorevoke-784fc430-ef03-409f-8989-d1d06954f515-3068f145-0e4b-423f-b2b8-4dce20238f50";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Notifications_Provider", "azure" },
                { "GovNotifyBaseUri", "https://api.notifications.service.gov.uk" },
                { "GovNotifyApiKey", revokedApiKey }
            })
            .Build();

        var serviceProvider = _serviceCollection
            .AddDependenciesNotUnderTest()
            .AddUserNotifications(configuration)
            .BuildServiceProvider();

        var notificationsClient = serviceProvider.GetService(typeof(ISendNotifications));
        notificationsClient.Should().BeOfType<GovNotifyClient>();
    }

    [Fact(DisplayName = "Binds GovNotifyRetryOptions from configuration")]
    public void NotificationsRegistration_BindsRetryOptions()
    {
        // Arrange: configuration for retry options
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
            { "Notifications_Provider", "azure" },
            { "GovNotifyBaseUri", "https://api.notifications.service.gov.uk" },
            { "GovNotifyApiKey", "some-api-key-123abc" },
            { "GovNotifyRetryOptions:MaxRetries", "5" },
            { "GovNotifyRetryOptions:InitialDelayMs", "1000" },
            { "GovNotifyRetryOptions:BackoffFactor", "2.0" }
            })
            .Build();

        var serviceProvider = _serviceCollection
            .AddDependenciesNotUnderTest()
            .AddUserNotifications(configuration)
            .BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<GovNotifyRetryOptions>>().Value;

        // Assert
        options.MaxRetries.Should().Be(5);
        options.InitialDelayMs.Should().Be(1000);
        options.BackoffFactor.Should().Be(2.0);
    }


}


public class FakeSiteSummaryAggregator : ISiteSummaryAggregator
{
    public Task AggregateForSite(string site, DateOnly from, DateOnly to) => throw new NotImplementedException();
}
