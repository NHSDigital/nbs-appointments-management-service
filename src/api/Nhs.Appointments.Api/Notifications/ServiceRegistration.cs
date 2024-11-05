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
                .AddTransient<IBookingMadeNotifier, BookingNotifier>()
                .AddTransient<IBookingReminderNotifier, BookingNotifier>()
                .AddScoped<NotifyBookingReminderFunction>();

        if (userNotificationsProvider == "local")
        {
            services
                .AddScoped<IConsumer<UserRolesChanged>, UserRolesChangedConsumer>()
                .AddScoped<IConsumer<BookingMade>, BookingMadeConsumer>()
                .AddScoped<IConsumer<BookingCancelled>, BookingCancelledConsumer>()
                .AddScoped<IConsumer<BookingReminder>, BookingReminderConsumer>()
                .AddScoped<IMessageBus, ConsoleLogWithMessageDelivery>()
                .AddScoped<ISendNotifications, FakeNotificationClient>();
        }
        else if(userNotificationsProvider == "azure")
        {
            services
                .AddScoped<ISendNotifications>(x => new GovNotifyEmailClient(Environment.GetEnvironmentVariable("GovNotifyApiKey")))
                .AddScoped<IMessageBus, MassTransitBusWrapper>()
                .AddScoped<NotifyUserRolesChangedFunction>()
                .AddScoped<NotifyBookingMadeFunction>()
                .AddScoped<NotifyBookingCancelledFunction>()
                .AddScoped<ScheduledBookingRemindersFunction>()
                .AddMassTransitForAzureFunctions(cfg =>
                {
                    EndpointConvention.Map<UserRolesChanged>(new Uri($"queue:{NotifyUserRolesChangedFunction.QueueName}"));
                    EndpointConvention.Map<BookingMade>(new Uri($"queue:{NotifyBookingMadeFunction.QueueName}"));
                    EndpointConvention.Map<BookingCancelled>(new Uri($"queue:{NotifyBookingCancelledFunction.QueueName}"));
                    EndpointConvention.Map<BookingReminder>(new Uri($"queue:{NotifyBookingReminderFunction.QueueName}"));
                    cfg.AddConsumer<UserRolesChangedConsumer>();
                    cfg.AddConsumer<BookingReminderConsumer>();
                    cfg.AddConsumer<BookingMadeConsumer>();
                    cfg.AddConsumer<BookingCancelledConsumer>();
                },
                connectionStringConfigurationKey: "ServiceBusConnectionString");
        }
        else
        {
            throw new NotSupportedException("A null or unsupported notifications provider was specified in the configuration");
        }

        return services;
    }
}
