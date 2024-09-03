using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;
using Notify.Client;
using Notify.Interfaces;
using System;

namespace Nhs.Appointments.Api.Notifications
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddUserNotifications(this IServiceCollection services)
        {
            var userNotificationsProvider = Environment.GetEnvironmentVariable("Notifications_Provider");

            services.Configure<UserRolesChangedNotifier.Options>(opts =>
            {
                opts.EmailTemplateId = Environment.GetEnvironmentVariable("UserRolesChangedEmailTemplateId");
            });

            
            if (userNotificationsProvider == "local")
            {
                services.AddScoped<IMessageBus, ConsoleLogNotifications>();
            }
            else if(userNotificationsProvider == "azure")
            {
                services
                    .AddTransient<IUserRolesChangedNotifier, UserRolesChangedNotifier>()
                    .AddScoped<IMessageBus, MassTransitBusWrapper>()                
                    .AddScoped<NotifyUserRolesChangedFunction>()
                    .AddMassTransitForAzureFunctions(cfg =>
                    {
                        cfg.UsingAzureServiceBus((context, c) =>
                        {
                            c.Host(Environment.GetEnvironmentVariable("ServiceBusConnectionString"));
                        });
                        cfg.AddConsumer<UserRolesChangedConsumer>();
                        cfg.AddRequestClient<UserRolesChanged>(new Uri("queue:user-role-change"));
                    })
                    .AddTransient<IAsyncNotificationClient>(x => new NotificationClient(Environment.GetEnvironmentVariable("GovNotifyApiKey")));
            }
            else
            {
                throw new NotSupportedException("A null or unsupported notifications provider was specified in the configuration");
            }

            return services;
        }
    }
}
