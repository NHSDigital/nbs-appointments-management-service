using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;

namespace Nhs.Appointments.Core
{
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
