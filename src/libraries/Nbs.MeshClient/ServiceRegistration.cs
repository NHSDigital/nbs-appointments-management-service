using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nbs.MeshClient.Auth;
using Nbs.MeshClient.Errors;
using Refit;

namespace Nbs.MeshClient;

/// <summary>
/// Service Registration
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Register Mesh
    /// </summary>
    public static IHttpClientBuilder AddMesh(this IServiceCollection services, IConfiguration configurationParent)
    {
        services
            .AddSingleton<IMeshFactory, MeshFactory>()
            .AddTransient<MeshErrorResponseHandler>()
            .Configure<MeshClientOptions>(configurationParent.GetSection(nameof(MeshClientOptions)));

        return services.AddRefitClient<IMeshClient>()
            .AddHttpMessageHandler<MeshErrorResponseHandler>()
            .AddSingleMailboxMeshAuthorizationHandler(configurationParent)
            .ConfigureHttpClient((services, client) =>
                client.BaseAddress =
                    new Uri(services.GetRequiredService<IOptions<MeshClientOptions>>().Value.BaseUrl));
    }
}
