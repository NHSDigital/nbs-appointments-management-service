using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Api;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Logger;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder
            .UseMiddleware<TypeDecoratorMiddleware>()            
            .UseMiddleware<AuthenticationMiddleware>()
            .UseMiddleware<AuthorizationMiddleware>()
            .ConfigureFunctionDependencies();
    })
    .UseAppointmentsSerilog()
    .Build();

host.Run();