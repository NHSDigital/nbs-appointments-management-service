using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Nhs.Appointments.Api.Auth
{
    public interface IRequestSigner
    {
        Task<string> SignRequestAsync(HttpRequestData requestData, string requestTimestamp, string key);
    }

    internal class RequestSigner : IRequestSigner
    {
        public async Task<string> SignRequestAsync(HttpRequestData requestData, string requestTimestamp, string key)
        {
            var method = requestData.Method;
            var path = requestData.Url.AbsolutePath;
            var queryString = requestData.Url.Query;
            var content = await requestData.ReadBodyAsStringAndLeaveIntactAsync();
            content += DecodeQueryString(queryString);
            var contentBytes = Encoding.UTF8.GetBytes(content);

            using var hash = SHA256.Create();
            var hashedContentBytes = hash.ComputeHash(contentBytes);
            var hashedContentBase64 = Convert.ToBase64String(hashedContentBytes);

            var hmacSha256 = new HMACSHA256 { Key = Convert.FromBase64String(key) };
            var payload = $"{method}\n{path}\n{requestTimestamp}\n{hashedContentBase64}";
            var sigBytes = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(sigBytes);
        }

        private static string DecodeQueryString(string queryString)
        {
            return queryString.Contains('?') ? HttpUtility.UrlDecode(queryString.Substring(1)) : string.Empty;
        }
    }
}
