using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Api;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.ConfigureFunctionDependencies();
    })
    .Build();

host.Run();
