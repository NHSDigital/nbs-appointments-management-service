using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Nhs.Appointments.Api.Auth
{
    internal static class RequestSigner
    {
        public static async Task<string> SignRequestAsync(HttpRequestData requestData, string requestTimestamp, string key)
        {            
            string method = requestData.Method;
            string path = requestData.Url.AbsolutePath;
            string content = await requestData.ReadBodyAsStringAndLeaveIntactAsync();
            content += GetCanonicalQueryParameters(requestData.Url.ParseQueryString());
            var contentBytes = Encoding.UTF8.GetBytes(content);
                        
            using var hash = SHA256.Create();
            var hashedContentBytes = hash.ComputeHash(contentBytes);
            var hashedContentBase64 = Convert.ToBase64String(hashedContentBytes);
            
            var hmacSha256 = new HMACSHA256 { Key = Convert.FromBase64String(key) };
            var payload = $"{method}\n{path}\n{requestTimestamp}\n{hashedContentBase64}";
            var sigBytes = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(sigBytes);
        }

        private static string GetCanonicalQueryParameters(NameValueCollection queryParameters) =>
            string.Join("&", queryParameters.AllKeys.Select(key => $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(queryParameters[key])}"));
    }
}
