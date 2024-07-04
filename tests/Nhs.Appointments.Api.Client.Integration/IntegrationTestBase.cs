using Nhs.Appointments.ApiClient;
using Nhs.Appointments.ApiClient.Auth;
using Nhs.Appointments.ApiClient.Impl;
using System.Net.Http.Headers;
using Moq;
using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Api.Client.Integration
{
    public abstract class IntegrationTestBase
    {
        private readonly INhsAppointmentsApi _apiClient;
        private string _signingKey;
        private const string GoodSigningKey = "2EitbEouxHQ0WerOy3TwcYxh3/wZA0LaGrU1xpKg0KJ352H/mK0fbPtXod0T0UCrgRHyVjF6JfQm/LillEZyEA==";
        private const string BadSigningKey = "dGhpc2lzd3Jvbmc=";
        protected const string KnownSiteId = "1";

        protected IntegrationTestBase()
        {
            UseGoodSigningKey();
            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole());

            ILogger<BookingsApiClient> bookingsLogger = loggerFactory.CreateLogger<BookingsApiClient>();
            ILogger<SitesApiClient> sitesLogger = loggerFactory.CreateLogger<SitesApiClient>();
            ILogger<TemplatesApiClient> templatesLogger = loggerFactory.CreateLogger<TemplatesApiClient>();

            _apiClient = new NhsAppointmentsApi(new BookingsApiClient(CreateHttpClient, bookingsLogger), new SitesApiClient(CreateHttpClient, sitesLogger), new TemplatesApiClient(CreateHttpClient, templatesLogger));
        }

        protected INhsAppointmentsApi ApiClient => _apiClient;

        private HttpClient CreateHttpClient()
        {
            var requestSigningHandler = new RequestSigningHandler(new RequestSigner(TimeProvider.System, _signingKey));
            requestSigningHandler.InnerHandler = new HttpClientHandler();
            var client = new HttpClient(requestSigningHandler);
            client.BaseAddress = new Uri("http://localhost:7071");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        protected void UseGoodSigningKey()
        {
            _signingKey = GoodSigningKey;
        }

        protected void UseBadSigningKey()
        {
            _signingKey = BadSigningKey;
        }
    }
}