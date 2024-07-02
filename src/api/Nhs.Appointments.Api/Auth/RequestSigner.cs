using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;
using Polly;
using System;
using System.Collections.Specialized;
using System.IO;
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
            string content = await requestData.ReadAsStringAsync();
            content += GetCanonicalQueryParameters(requestData.Url.ParseQueryString());
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var copiedStream = new MemoryStream(contentBytes);

            // Reading from the requestData means that other parts of the processing chain cannot, hence we have to copy it and use reflection to reset it. 
            var httpRequestField = requestData.GetType().GetField("_httpRequest", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var httpRequest = httpRequestField.GetValue(requestData) as HttpRequest;
            httpRequest.Body = copiedStream;
            
            using var hash = SHA256.Create();
            var hashedContentBytes = hash.ComputeHash(contentBytes);
            var hashedContentBase64 = Convert.ToBase64String(hashedContentBytes);

            
            var hmacSha256 = new HMACSHA256 { Key = Convert.FromBase64String(key) };
            var payload = $"{method}\n{path}\n{requestTimestamp}\n{hashedContentBase64}";
            var sigBytes = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(sigBytes);
        }

        private static string GetCanonicalQueryParameters(NameValueCollection queryParameters)
        {
            StringBuilder canonicalQueryParameters = new StringBuilder();
            foreach (string key in queryParameters)
            {
                canonicalQueryParameters.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(queryParameters[key]));
            }

            // remove trailing '&'
            if (canonicalQueryParameters.Length > 0)
                canonicalQueryParameters.Remove(canonicalQueryParameters.Length - 1, 1);

            return canonicalQueryParameters.ToString();
        }
    }
}
