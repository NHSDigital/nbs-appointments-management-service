using System.Security.Cryptography.X509Certificates;

namespace Nbs.MeshClient.Auth
{
    public interface ICertificateProvider
    {
        Task<X509Certificate2> GetClientCertificateAsync(string certificateName);
    }    
}
