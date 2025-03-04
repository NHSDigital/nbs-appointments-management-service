using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Nhs.Appointments.Api;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Logger;
using Nhs.Appointments.Api.Middleware;
using Nhs.Appointments.Audit;

var host = new HostBuilder()
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        var azureAppConfigConnection = Environment.GetEnvironmentVariable("AzureAppConfigConnection");
        if (azureAppConfigConnection == "local")
        {
            cfg.AddJsonFile("local.feature.flags.json", optional: false, reloadOnChange: true);
        }
        else
        {
            cfg.AddAzureAppConfiguration(options =>
            {
                options.Connect(azureAppConfigConnection)
                    .UseFeatureFlags()
                    .ConfigureRefresh(refresh => refresh.RegisterAll().SetRefreshInterval(TimeSpan.FromMinutes(10)));
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
        
        services.AddSingleton<ITargetingContextAccessor, TargetingContextAccessor>();
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
