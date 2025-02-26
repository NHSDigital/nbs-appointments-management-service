using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Nhs.Appointments.Api;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Logger;
using Nhs.Appointments.Api.Middleware;
using Nhs.Appointments.Audit;

var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        services.AddFeatureManagement();
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
