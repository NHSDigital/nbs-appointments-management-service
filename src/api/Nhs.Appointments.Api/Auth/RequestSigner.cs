using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Auth
{
    internal static class RequestSigner
    {
        public static async Task<string> SignRequestAsync(HttpRequestData requestData, string requestTimestamp, string key)
        {            
            string method = requestData.Method;
            string path = requestData.Url.AbsolutePath;

            var contentBytes = await requestData.ReadBodyAsBytesAndLeaveIntactAsync();
            
            using var hash = SHA256.Create();
            var hashedContentBytes = hash.ComputeHash(contentBytes);
            var hashedContentBase64 = Convert.ToBase64String(hashedContentBytes);
            
            var hmacSha256 = new HMACSHA256 { Key = Convert.FromBase64String(key) };
            var payload = $"{method}\n{path}\n{requestTimestamp}\n{hashedContentBase64}";
            var sigBytes = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(sigBytes);
        }
    }

    public static class HttpRequestDataExtensions
    {
        public static async Task<byte[]> ReadBodyAsBytesAndLeaveIntactAsync(this HttpRequestData httpRequestData)
        {
            string content = await httpRequestData.ReadAsStringAsync();
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var copiedStream = new MemoryStream(contentBytes);

            // Reading from the requestData means that other parts of the processing chain cannot, hence we have to copy it and use reflection to reset it. 
            var httpRequestField = httpRequestData.GetType().GetField("_httpRequest", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var httpRequest = httpRequestField.GetValue(httpRequestData) as HttpRequest;
            httpRequest.Body = copiedStream;

            return contentBytes;
        }

        public static async Task<string> ReadBodyAsStringAndLeaveIntactAsync(this HttpRequestData httpRequestData)
        {
            var bytes = await ReadBodyAsBytesAndLeaveIntactAsync(httpRequestData);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
