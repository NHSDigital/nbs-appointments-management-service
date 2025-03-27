using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

namespace Nhs.Appointments.Api.Features;

public static class FeatureConfigurationExtensions
{
    public static IHostBuilder ConfigureFeatureDependencies(this IHostBuilder builder)
    {
        IConfigurationRefresher configRefresher = null;

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            var azureAppConfigConnection = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION");
            if (azureAppConfigConnection == "local")
            {
                configRefresher = new LocalConfigurationRefresher();
                var configPath = Path.Combine(AppContext.BaseDirectory, "local.feature.flags.json");
                cfg.AddJsonFile(configPath, optional: false, reloadOnChange: true);
            }
            else
            {
                cfg.AddAzureAppConfiguration(options =>
                {
                    options.Connect(azureAppConfigConnection)
                        .UseFeatureFlags()
                        .ConfigureRefresh(refresh =>
                        {
                            refresh.RegisterAll()
                                .SetRefreshInterval(TimeSpan.FromMinutes(10));
                        });

                    //capture refresher and register it
                    configRefresher = options.GetRefresher();
                });
            }
        });

        builder.ConfigureServices(services =>
        {
            services
                .AddFeatureManagement()
                .AddFeatureFilter<PercentageFilter>()
                .AddFeatureFilter<TargetingFilter>()
                .AddFeatureFilter<TimeWindowFilter>();

            services.AddSingleton(configRefresher);
            services.AddSingleton<ITargetingContextAccessor, DefaultContextAccessor>();
        });
        
        return builder;
    }
}
