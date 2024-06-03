using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Api;
using Nhs.Appointments.Api.Auth;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder
            .UseMiddleware<TypeDecoratorMiddleware>()
            .UseMiddleware<AuthenticationMiddleware>()
            .ConfigureFunctionDependencies();
    })
    .Build();

host.Run();
