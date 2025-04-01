using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Api;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Features;
using Nhs.Appointments.Api.Logger;
using Nhs.Appointments.Api.Middleware;
using Nhs.Appointments.Audit;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddUserSecrets<Program>();
    })
    .ConfigureFeatureDependencies()
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
