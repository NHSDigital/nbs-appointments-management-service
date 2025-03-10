using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.Notifications;

namespace Nhs.Appointments.Core;

public static class ServiceRegistration
{
    public static IServiceCollection AddRequestInspectors(this IServiceCollection services)
    {
        var inspectorTypes = typeof(IRequestInspector).Assembly
            .GetTypes()
            .Where(t => typeof(IRequestInspector).IsAssignableFrom(t) && t.IsClass && t.IsAbstract == false);

        foreach (var type in inspectorTypes)
        {
            services.AddSingleton(type);
        }

        return services;
    }

    public static IServiceCollection AddCommsOptions(this IServiceCollection services)
    {
        // Set up configuration
        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables();
        var configuration = builder.Build();

        services.Configure<CommsOptions>(opts => configuration.GetSection("Comms").Bind(opts));

        return services;
    }
}
