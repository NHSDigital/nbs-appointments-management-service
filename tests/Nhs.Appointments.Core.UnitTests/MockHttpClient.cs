using Newtonsoft.Json;
using System.Net;

namespace Nhs.Appointments.Core.UnitTests
{
    public class MockHttpClient : HttpMessageHandler
    {        
        private readonly HttpClient _httpClient;
        private readonly Queue<HttpResponseMessage> _responseQueue;

        public MockHttpClient()
        {
            _httpClient = new HttpClient(this);
            _httpClient.BaseAddress = new Uri("http://test.com:1000");
            _responseQueue = new Queue<HttpResponseMessage>();
        }

        public HttpClient Client => _httpClient;

        public void EnqueueResponse(HttpResponseMessage response)
        {
            _responseQueue.Enqueue(response);
        }

        public void EnqueueResponse(HttpStatusCode status)
        {
            EnqueueResponse(new HttpResponseMessage(status));
        }

        public void EnqueueJsonResponse<TPayload>(TPayload payload)
        {
            var mockApiResponse = new HttpResponseMessage()
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload))
            };
            EnqueueResponse(mockApiResponse);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseQueue.Dequeue());
        }
    }

}