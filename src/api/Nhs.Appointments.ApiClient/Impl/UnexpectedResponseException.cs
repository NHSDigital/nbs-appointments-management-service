using System.Net;

namespace Nhs.Appointments.ApiClient.Impl
{
    public class UnexpectedResponseException : Exception 
    {
        public UnexpectedResponseException(HttpStatusCode received, HttpStatusCode[] expected)
            : base($"Recieved HTTP status code {(int)received} {received}. Expected: {string.Join(',', expected)}")
        {

        }
    }
}
