using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Api;
using Nhs.Appointments.Api.Auth;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder
            .UseMiddleware<TypeDecoratorMiddleware>()
            .UseMiddleware<AuthenticationMiddleware>()
            .UseMiddleware<AuthorizationMiddleware>()
            .ConfigureFunctionDependencies();
    })
    .Build();

host.Run();
