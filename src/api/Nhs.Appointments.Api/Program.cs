using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Api;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Logger;
using Nhs.Appointments.Audit;
using Nhs.Appointments.Audit.Functions;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder
            .UseMiddleware<TypeDecoratorMiddleware>()
            .UseMiddleware<AuthenticationMiddleware>()
            .UseMiddleware<AuthorizationMiddleware>()
            .AddAudit()
            .ConfigureFunctionDependencies();
    })
    .UseAppointmentsSerilog()
    .Build();

host.Run();
