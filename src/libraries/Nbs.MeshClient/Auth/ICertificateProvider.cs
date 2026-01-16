using System.Security.Cryptography.X509Certificates;

namespace Nbs.MeshClient.Auth;

/// <summary>
///     CertificateProvider interface
/// </summary>
public interface ICertificateProvider
{
    /// <summary>
    ///     GetClientCertificateAsync
    /// </summary>
    Task<X509Certificate2> GetClientCertificateAsync(string certificateName);
}
