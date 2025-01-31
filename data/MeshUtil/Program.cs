using MeshUtil;
using Nbs.MeshClient;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
.AddNbsAzureKeyVault();

var configuration = (builder.Configuration as IConfigurationBuilder).Build();

builder.Services
    .AddHostedService<MeshUtilWorker>()
    .AddCertificateProvider(configuration)
    .AddMesh(configuration);

var host = builder.Build();
host.Run();
