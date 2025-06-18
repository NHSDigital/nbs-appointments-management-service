using System;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;
using Notify.Client;

namespace Nhs.Appointments.Api.Notifications;

public static class ServiceRegistration
{
    public static IServiceCollection AddUserNotifications(this IServiceCollection services,
        IConfigurationRoot configuration)
    {
        var notificationsConfig = configuration.Get<NotificationsConfig>();
        var oktaEnabled = configuration.GetValue<bool>($"FeatureManagement:{Flags.OktaEnabled}");

        services.AddTransient<IUserRolesChangedNotifier, UserRolesChangedNotifier>()
                .AddTransient<IBookingNotifier, BookingNotifier>()
                .AddTransient<IPrivacyUtil, PrivacyUtil>()
                .AddScoped<NotifyBookingReminderFunction>();

        switch (notificationsConfig.NotificationsProvider)
        {
            case "none":
                services.AddScoped<IMessageBus, NullMessageBus>();
                break;
            case "local":
                services
                    .AddScoped<IConsumer<UserRolesChanged>, UserRolesChangedConsumer>()
                    .AddScoped<IConsumer<OktaUserRolesChanged>, OktaUserRolesChangedConsumer>()
                    .AddScoped<IConsumer<BookingMade>, BookingMadeConsumer>()
                    .AddScoped<IConsumer<BookingCancelled>, BookingCancelledConsumer>()
                    .AddScoped<IConsumer<BookingReminder>, BookingReminderConsumer>()
                    .AddScoped<IConsumer<BookingRescheduled>, BookingRescheduledConsumer>()
                    .AddScoped<IMessageBus, ConsoleLogWithMessageDelivery>()
                    .AddScoped<ISendNotifications, CosmosNotificationClient>();
                break;
            case "azure-throttled":
                services
                    .AddScoped<ISendNotifications, FakeNotificationClient>()
                    .AddNotificationFunctions()
                    .AddMassTransit(oktaEnabled);
                break;
            case "azure":
                services
                    .AddScoped(x =>
                        new NotificationClient(notificationsConfig.GovNotifyBaseUri,
                            notificationsConfig.GovNotifyApiKey))
                    .AddScoped<ISendNotifications, GovNotifyClient>()
                    .AddNotificationFunctions()
                    .AddMassTransit(oktaEnabled);
                break;
            default:
                throw new NotSupportedException(
                    "A null or unsupported notifications provider was specified in the configuration");
        }

        return services;
    }

    private static IServiceCollection AddNotificationFunctions(this IServiceCollection services)
    {
        services
            .AddScoped<NotifyUserRolesChangedFunction>()
            .AddScoped<NotifyBookingMadeFunction>()
            .AddScoped<NotifyBookingCancelledFunction>()
            .AddScoped<ScheduledBookingRemindersFunction>()
            .AddScoped<NotifyBookingRescheduledFunction>()
            .AddScoped<NotifyOktaUserRolesChangedFunction>();
        return services;
    }

    private static IServiceCollection AddMassTransit(this IServiceCollection services, bool oktaEnabled)
    {
        services
            .AddScoped<IMessageBus, MassTransitBusWrapper>()
            .AddMassTransitForAzureFunctions(cfg =>
            {
                EndpointConvention.Map<UserRolesChanged>(new Uri($"queue:{NotifyUserRolesChangedFunction.QueueName}"));
                EndpointConvention.Map<BookingMade>(new Uri($"queue:{NotifyBookingMadeFunction.QueueName}"));
                EndpointConvention.Map<BookingRescheduled>(new Uri($"queue:{NotifyBookingRescheduledFunction.QueueName}"));
                EndpointConvention.Map<BookingCancelled>(new Uri($"queue:{NotifyBookingCancelledFunction.QueueName}"));
                EndpointConvention.Map<BookingReminder>(new Uri($"queue:{NotifyBookingReminderFunction.QueueName}"));

                cfg.AddConsumer<UserRolesChangedConsumer>();
                cfg.AddConsumer<BookingReminderConsumer>();
                cfg.AddConsumer<BookingMadeConsumer>();
                cfg.AddConsumer<BookingCancelledConsumer>();
                cfg.AddConsumer<BookingRescheduledConsumer>();

                if (oktaEnabled)
                {
                    EndpointConvention.Map<OktaUserRolesChanged>(
                        new Uri($"queue:{NotifyOktaUserRolesChangedFunction.QueueName}"));
                    cfg.AddConsumer<OktaUserRolesChangedConsumer>();
                }
            },
            "ServiceBusConnectionString");
        return services;
    }
}
