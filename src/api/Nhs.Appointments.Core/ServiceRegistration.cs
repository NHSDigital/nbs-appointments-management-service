using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core.Inspectors;

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
}
