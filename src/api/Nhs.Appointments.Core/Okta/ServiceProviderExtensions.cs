using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Okta.Sdk.Api;
using Okta.Sdk.Client;

namespace Nhs.Appointments.Core.Okta;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddOktaUserDirectory(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var oktaConfig = configuration.GetSection("Okta").Get<OktaConfiguration>();

        services.Configure<OktaConfiguration>(opts => configuration.GetSection("Okta").Bind(opts));
        services.AddSingleton<IOktaService, OktaService>();

        if (oktaConfig is
            { Domain: "https://local.okta.com", ManagementId: "local", PEM: "local", PrivateKeyKid: "local" })
        {
            services.AddSingleton<IOktaUserDirectory, OktaLocalUserDirectory>();
            return services;
        }

        services.AddSingleton<IOktaUserDirectory, OktaUserDirectory>();
        services.AddSingleton<UserApi>(sp =>
        {
            var oktaOptions = sp.GetRequiredService<IOptions<OktaConfiguration>>().Value;

            var rsa = RSA.Create();
            rsa.ImportFromPem(oktaOptions.PEM);
            var keyParams = rsa.ExportParameters(true);

            var privateKey = new JsonWebKeyConfiguration
            {
                P = Convert.ToBase64String(keyParams.P),
                Kty = "RSA",
                Q = Convert.ToBase64String(keyParams.Q),
                D = Convert.ToBase64String(keyParams.D),
                E = Convert.ToBase64String(keyParams.Exponent),
                Kid = oktaOptions.PrivateKeyKid,
                Qi = Convert.ToBase64String(keyParams.InverseQ),
                Dp = Convert.ToBase64String(keyParams.DP),
                Dq = Convert.ToBase64String(keyParams.DQ),
                N = Convert.ToBase64String(keyParams.Modulus),
            };

            return new UserApi(new Configuration
            {
                OktaDomain = oktaOptions.Domain,
                AuthorizationMode = AuthorizationMode.PrivateKey,
                ClientId = oktaOptions.ManagementId,
                Scopes = new HashSet<string> { "okta.users.read", "okta.users.manage" },
                PrivateKey = privateKey,
            });
        });

        return services;
    }
}
