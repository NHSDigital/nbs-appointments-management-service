using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;
using System;

namespace Nhs.Appointments.Api.Notifications;

public static class ServiceRegistration
{
    public static IServiceCollection AddUserNotifications(this IServiceCollection services)
    {
        var userNotificationsProvider = Environment.GetEnvironmentVariable("Notifications_Provider");

        services.AddTransient<IUserRolesChangedNotifier, UserRolesChangedNotifier>()
                .AddTransient<IBookingNotifier, BookingNotifier>()
                .AddTransient<IPrivacyUtil, PrivacyUtil>()
                .AddScoped<NotifyBookingReminderFunction>();

        switch (userNotificationsProvider)
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
            case "azure":
                var notifyBaseUri = Environment.GetEnvironmentVariable("GovNotifyBaseUri");
                var notifyApiKey = Environment.GetEnvironmentVariable("GovNotifyApiKey");
                services
                    .AddScoped(x => new Notify.Client.NotificationClient(notifyBaseUri, notifyApiKey))
                    .AddScoped<ISendNotifications, GovNotifyClient>()
                    .AddScoped<IMessageBus, MassTransitBusWrapper>()
                    .AddScoped<NotifyUserRolesChangedFunction>()
                    .AddScoped<NotifyBookingMadeFunction>()
                    .AddScoped<NotifyBookingCancelledFunction>()
                    .AddScoped<ScheduledBookingRemindersFunction>()
                    .AddScoped<NotifyBookingRescheduledFunction>()
                    .AddMassTransitForAzureFunctions(cfg =>
                        {
                            EndpointConvention.Map<UserRolesChanged>(new Uri($"queue:{NotifyUserRolesChangedFunction.QueueName}"));
                            EndpointConvention.Map<OktaUserRolesChanged>(new Uri($"queue:{NotifyOktaUserRolesChangedFunction.QueueName}"));
                            EndpointConvention.Map<BookingMade>(new Uri($"queue:{NotifyBookingMadeFunction.QueueName}"));
                            EndpointConvention.Map<BookingRescheduled>(new Uri($"queue:{NotifyBookingRescheduledFunction.QueueName}"));
                            EndpointConvention.Map<BookingCancelled>(new Uri($"queue:{NotifyBookingCancelledFunction.QueueName}"));
                            EndpointConvention.Map<BookingReminder>(new Uri($"queue:{NotifyBookingReminderFunction.QueueName}"));
                            cfg.AddConsumer<UserRolesChangedConsumer>();
                            cfg.AddConsumer<OktaUserRolesChangedConsumer>();
                            cfg.AddConsumer<BookingReminderConsumer>();
                            cfg.AddConsumer<BookingMadeConsumer>();
                            cfg.AddConsumer<BookingCancelledConsumer>();
                            cfg.AddConsumer<BookingRescheduledConsumer>();
                        },
                        connectionStringConfigurationKey: "ServiceBusConnectionString");
                break;
            default:
                throw new NotSupportedException("A null or unsupported notifications provider was specified in the configuration");
        }
        return services;
    }
}
