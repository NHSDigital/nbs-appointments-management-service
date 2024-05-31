using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Api;
using Nhs.Appointments.Api.Auth;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseMiddleware<AuthenticationMiddleware>();
        builder.ConfigureFunctionDependencies();
    })
    .Build();

host.Run();
