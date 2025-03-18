using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using Nbs.MeshClient.Auth;
using System.Security.Cryptography.X509Certificates;

namespace DataExtract;

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
