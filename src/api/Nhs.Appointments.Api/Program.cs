using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Nhs.Appointments.Api;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Logger;
using Nhs.Appointments.Api.Middleware;
using Nhs.Appointments.Audit;

var host = new HostBuilder()
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        var azureAppConfigConnection = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION");
        if (azureAppConfigConnection == "local")
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "local.feature.flags.json");
            cfg.AddJsonFile(configPath, optional: false, reloadOnChange: true);
        }
        else
        {
            cfg.AddAzureAppConfiguration(options =>
            {
                options
                    .Connect(azureAppConfigConnection)
                    .UseFeatureFlags()
                    .ConfigureRefresh(refresh =>
                    {
                        refresh.RegisterAll().SetRefreshInterval(TimeSpan.FromMinutes(1));
                    });
            });
        }
    })
    .ConfigureServices(services =>
    {
        services
            .AddFeatureManagement()
            .AddFeatureFilter<PercentageFilter>()
            .AddFeatureFilter<TargetingFilter>()
            .AddFeatureFilter<TimeWindowFilter>();

        services.AddSingleton<ITargetingContextAccessor, DefaultContextAccessor>();
    })
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder
            .UseMiddleware<TypeDecoratorMiddleware>()
            .UseMiddleware<AuthenticationMiddleware>()
            .UseMiddleware<AuthorizationMiddleware>()
            .UseMiddleware<NoCacheMiddleware>()
            .AddAudit()
            .ConfigureFunctionDependencies();
    })
    .UseAppointmentsSerilog()
    .Build();

host.Run();
