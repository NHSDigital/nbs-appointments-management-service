using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using Nbs.MeshClient.Auth;
using System.Security.Cryptography.X509Certificates;

namespace MeshUtil;
public static class ServiceRegistration
{
    public static IConfigurationBuilder AddNbsAzureKeyVault(this IConfigurationBuilder builder, string configSectionName = AzureKeyVaultConfiguration.DefaultConfigSectionName)
    {
        var tempConfig = builder.Build();
        var config = tempConfig.GetAzureKeyVaultConfiguration(configSectionName);

        if (config is null || string.IsNullOrEmpty(config?.KeyVaultName))
        {
            Console.WriteLine("Failed to add key vault config");
            return builder;
        }

        Console.WriteLine("Added key vault config");
        var keyVaultUri = new Uri($"https://{config.KeyVaultName}.vault.azure.net/");
        var credential = new ClientSecretCredential(config.TenantId, config.ClientId, config.ClientSecret);

        return builder.AddAzureKeyVault(keyVaultUri, credential);
    }

    public static IServiceCollection AddCertificateProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var azureKeyVaultConfig = configuration.GetSection("KeyVault")?.Get<AzureKeyVaultConfiguration>();
        if (!string.IsNullOrEmpty(azureKeyVaultConfig?.KeyVaultName))
        {
            Console.WriteLine("Adding key vault and cert provider");
            services.Configure<AzureKeyVaultConfiguration>(opts =>
            {
                opts.KeyVaultName = azureKeyVaultConfig.KeyVaultName;
                opts.ClientId = azureKeyVaultConfig.ClientId;
                opts.TenantId = azureKeyVaultConfig.TenantId;
                opts.ClientSecret = azureKeyVaultConfig.ClientSecret;
            });
            services.AddSingleton<ICertificateProvider, KeyVaultCertificateProvider>();
        }
        else
            Console.WriteLine("Key vault configuration not set up");
        return services;
    }

    public static AzureKeyVaultConfiguration? GetAzureKeyVaultConfiguration(this IConfiguration configuration, string configSectionName = AzureKeyVaultConfiguration.DefaultConfigSectionName)
    {
        return configuration.GetSection(configSectionName)?.Get<AzureKeyVaultConfiguration>();
    }
}

public class KeyVaultCertificateProvider(IOptions<AzureKeyVaultConfiguration> providerOptions) : ICertificateProvider
{
    public async Task<X509Certificate2> GetClientCertificateAsync(string certificateName)
    {
        var keyVaultUri = new Uri($"https://{providerOptions.Value.KeyVaultName}.vault.azure.net/");
        var credential = new ClientSecretCredential(providerOptions.Value.TenantId, providerOptions.Value.ClientId, providerOptions.Value.ClientSecret);
        var certificateClient = new SecretClient(keyVaultUri, credential);
        var result = await certificateClient.GetSecretAsync(certificateName);
        var certBytes = Convert.FromBase64String(result.Value.Value);
        return new X509Certificate2(certBytes);
    }
}

public record AzureKeyVaultConfiguration
{
    public const string DefaultConfigSectionName = "KeyVault";

    public required string KeyVaultName { get; set; }
    public required string TenantId { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}
