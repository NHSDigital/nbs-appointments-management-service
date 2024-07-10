using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Nhs.Appointments.ApiClient.Auth
{
    public class RequestSigner(TimeProvider time, string signingKey) : IRequestSigner
    {
        public async Task SignRequestAsync(HttpRequestMessage request)
        {           
            string method = request.Method.ToString();
            string path = request.RequestUri!.AbsolutePath;
            string content = "";
            if (request.Content != null)
                content = await request.Content.ReadAsStringAsync();
            content += GetCanonicalQueryParameters(request.RequestUri.ParseQueryString());

            using var hash = SHA256.Create();
            var hashedContentBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(content));
            var hashedContentBase64 = Convert.ToBase64String(hashedContentBytes);

            var hmacSha256 = new HMACSHA256 { Key = Convert.FromBase64String(signingKey) };
            var requestTimestamp = time.GetUtcNow().ToString("o", CultureInfo.InvariantCulture);
            var payload = $"{method}\n{path}\n{requestTimestamp}\n{hashedContentBase64}";
            var sigBytes = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payload));

            request.Headers.Authorization = new("Signed", Convert.ToBase64String(sigBytes));
            request.Headers.Add("RequestTimestamp", requestTimestamp);
        }

        private static string GetCanonicalQueryParameters(NameValueCollection queryParameters) => 
            string.Join("&", queryParameters.AllKeys.Select(key => $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(queryParameters[key])}"));
    }
}
