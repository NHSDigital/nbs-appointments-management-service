using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Api.Notifications.Options;
using Notify.Client;
using Notify.Interfaces;

namespace JobRunner.Job.Notify;

public static class ServiceRegistration
{
    public static IServiceCollection AddNotifyJob(this IServiceCollection services,
        IConfigurationBuilder configurationBuilder)
    {
        var configuration = configurationBuilder.Build();

        services
            .Configure<NotifySendOptions>(config =>
            {
                config.PhoneTemplateId = configuration.GetValue<string>("PhoneTemplateId") ?? throw new ArgumentNullException("PhoneTemplateId");
                config.EmailTemplateId = configuration.GetValue<string>("EmailTemplateId") ?? throw new ArgumentNullException("EmailTemplateId");
            })
            .AddGovNotify(configuration)
            .AddAzureBlobStorage(configuration)
            .AddTransient<INotifyInfoReader<BookingInfo>, BookingInfoReader>()
            .AddTransient<ISendTracker, BlobSendTracker>()
            .AddHostedService<Worker>();
        
        return services;
    }

    private static IServiceCollection AddAzureBlobStorage(this IServiceCollection services,
        IConfiguration configuration)
    {
        var mode = configuration["BlobStorageMode"];

        switch (mode)
        {
            case "local":
                services.AddTransient<IAzureBlobStorage, LocalBlobStorage>();
                break;
            case "azure":
                services
                    .AddTransient<IAzureBlobStorage, AzureBlobStorage>()
                    .AddAzureClients(x =>
                    {
                        x.AddBlobServiceClient(configuration["BlobStorageConnectionString"]);
                    });
                break;
            default:
                throw new NotSupportedException(
                    "A null or unsupported Azure Blob Storage Mode was specified in the configuration");
        }
        
        return services;
    }

    private static IServiceCollection AddGovNotify(this IServiceCollection services, IConfigurationRoot config)
    {
        var notificationsConfig = config.Get<NotificationsConfig>();

        switch (notificationsConfig.NotificationsProvider)
        {
            case "local":
                services
                    .AddScoped<ISendNotifications, FakeNotificationClient>();
                break;
            case "live":
                services
                    .Configure<GovNotifyRetryOptions>(
                        config.GetSection("GovNotifyRetryOptions")
                    )
                    .AddTransient<IPrivacyUtil, PrivacyUtil>()
                    .AddTransient<ISendNotifications, GovNotifyClient>()
                    .AddScoped<IAsyncNotificationClient>(x =>
                        new NotificationClient(
                            notificationsConfig.GovNotifyBaseUri,
                            notificationsConfig.GovNotifyApiKey
                        ));
                break;
            default: 
                throw new NotSupportedException(
                    "A null or unsupported notifications provider was specified in the configuration");
        }
        
        return services;
    }
}
