using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nbs.MeshClient.Auth;

/// <summary>
/// ServiceRegistration
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Register Single Mailbox Mesh Authorization Handler
    /// </summary>
    public static IHttpClientBuilder AddSingleMailboxMeshAuthorizationHandler(
        this IHttpClientBuilder httpClientBuilder, IConfiguration config)
    {
        httpClientBuilder.Services
            .AddSingleton<IMeshAuthorizationTokenGenerator, MeshAuthorizationTokenGenerator>()
            .AddTransient<MeshAuthorizationHandler>()
            .Configure<MeshAuthorizationOptions>(config.GetSection(nameof(MeshAuthorizationOptions)));

        return httpClientBuilder
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var handler = new HttpClientHandler();

                var certificateProvider = sp.GetService<ICertificateProvider>();
                var certificateName = config.GetSection(nameof(MeshAuthorizationOptions))
                    .GetValue<string>(nameof(MeshAuthorizationOptions.CertificateName));

                if (certificateProvider is not null && !string.IsNullOrEmpty(certificateName))
                {
                    var meshCertificate = certificateProvider.GetClientCertificateAsync(certificateName)
                        .GetAwaiter().GetResult();
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ClientCertificates.Add(meshCertificate);
                    handler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) =>
                    {
                        return true;
                    };
                }

                return handler;
            })
            .AddHttpMessageHandler<MeshAuthorizationHandler>();
    }
}
