using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;
using Notify.Interfaces;
using System;

namespace Nhs.Appointments.Api.Notifications;

public static class ServiceRegistration
{
    public static IServiceCollection AddUserNotifications(this IServiceCollection services)
    {
        var userNotificationsProvider = Environment.GetEnvironmentVariable("Notifications_Provider");

        services.Configure<UserRolesChangedNotifier.Options>(opts =>
        {
            opts.EmailTemplateId = Environment.GetEnvironmentVariable("UserRolesChangedEmailTemplateId");
        });

        services.AddScoped<IAsyncNotificationClient>(x => new Notify.Client.NotificationClient(Environment.GetEnvironmentVariable("GovNotifyApiKey")));
   
        if (userNotificationsProvider == "local")
        {
            services
                .AddTransient<IUserRolesChangedNotifier, UserRolesChangedNotifier>()
                .AddScoped<IConsumer<UserRolesChanged>, UserRolesChangedConsumer>()
                .AddScoped<IMessageBus, ConsoleLogWithMessageDelivery>();
        }
        else if(userNotificationsProvider == "azure")
        {
            services
                .AddTransient<IUserRolesChangedNotifier, UserRolesChangedNotifier>()
                .AddScoped<IMessageBus, MassTransitBusWrapper>()
                .AddScoped<NotifyUserRolesChangedFunction>()
                .AddMassTransitForAzureFunctions(cfg =>
                {
                    EndpointConvention.Map<UserRolesChanged>(new Uri("queue:user-roles-changed"));
                    cfg
                    .AddConsumer<UserRolesChangedConsumer>();
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
