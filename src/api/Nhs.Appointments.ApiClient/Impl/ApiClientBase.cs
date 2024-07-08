using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.ApiClient.Configuration;
using Nhs.Appointments.ApiClient.Models;
using System.Net;
using System.Net.Http.Json;

namespace Nhs.Appointments.ApiClient.Impl
{
    public abstract class ApiClientBase(Func<HttpClient> httpClientFactory, ILogger logger)
    {
        protected async Task<TResponse> Get<TResponse>(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await SendAsync(request);
            AssertResponseStatusCode(response, HttpStatusCode.OK);
            try
            {
                var responseObject = await response.Content.ReadFromJsonAsync<TResponse>();
          
                return responseObject;
            }
            catch(System.Text.Json.JsonException ex)
            {
                var json = await response.Content.ReadAsStringAsync();
                throw new ModelException($"Json error: {json}", ex);
            }
        }

        protected async Task<TResponse> Post<TRequest, TResponse>(string path, TRequest requestObject)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(requestObject)
            };

            var response = await SendAsync(request);
            AssertResponseStatusCode(response, HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.Created);

            try
            {
                var responseObject = await response.Content.ReadFromJsonAsync<TResponse>();
                return responseObject;
            }
            catch (System.Text.Json.JsonException ex)
            {
                var json = await response.Content.ReadAsStringAsync();
                throw new ModelException($"Json error: {json}", ex);
            }
        }

        protected async Task Post<TRequest>(string path, TRequest requestObject)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(requestObject)
            };

            var response = await SendAsync(request);
            AssertResponseStatusCode(response, HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.Created);
        }

        protected async Task Post(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path);

            var response = await SendAsync(request);
            AssertResponseStatusCode(response, HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.Created);
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            using(var http = httpClientFactory())
            {
                var response = await http.SendAsync(request);
                logger.LogTrace($"{(int)response.StatusCode} {response.StatusCode} {request.Method} {request.RequestUri}");
                return response;
            }        
        }

        private async void AssertResponseStatusCode(HttpResponseMessage response, params HttpStatusCode[] expected)
        {
            if(!expected.Contains(response.StatusCode))
            {
                var exception = new UnexpectedResponseException(response.StatusCode, expected);
                var responseContent = await response.Content.ReadAsStringAsync();
                logger.LogTrace(responseContent);
                logger.LogError(exception.Message);
                throw exception;
            }
        }     
    }
}
