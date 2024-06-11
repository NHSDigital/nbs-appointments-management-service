using System.Security.Cryptography;
using System.Text;

namespace Nhs.Appointments.ApiClient
{

    internal static class RequestSigner
    {
        public static async Task<string> SignRequestAsync(HttpRequestMessage request, string requestTimestamp, string key)
        {
            string method = request.Method.ToString();
            string path = request.RequestUri!.AbsolutePath;
            string content = "";
            if (request.Content != null)
                content = await request.Content.ReadAsStringAsync();

            using var hash = SHA256.Create();
            var hashedContentBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(content));
            var hashedContentBase64 = Convert.ToBase64String(hashedContentBytes);

            var hmacSha256 = new HMACSHA256 { Key = Convert.FromBase64String(key) };

            var payload = $"{method}\n{path}\n{requestTimestamp}\n{hashedContentBase64}";
            var sigBytes = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(sigBytes);
        }
    }
}
