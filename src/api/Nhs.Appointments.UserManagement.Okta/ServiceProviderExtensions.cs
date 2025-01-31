using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.UserManagement.Okta;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddOktaUserDirectory(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.Configure<OktaConfiguration>(opts => configuration.GetSection("Okta").Bind(opts));
        services.AddSingleton<IUserDirectory, OktaUserDirectory>();
        return services;
    }
}
