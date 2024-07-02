using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Nhs.Appointments.ApiClient.Auth;
using Nhs.Appointments.ApiClient.Impl;
using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.ApiClient.Configuration
{
    public static class ContainerConfiguration
    {
        public static IServiceCollection AddNhsAppointmentsApiClient(this IServiceCollection services, Uri endpoint, string signingKey)
        {
            services.AddTransient<INhsAppointmentsApi, NhsAppointmentsApi>();
            services.AddTransient<IBookingsApiClient, BookingsApiClient>();
            services.AddTransient<ISitesApiClient, SitesApiClient>();
            services.AddTransient<ITemplatesApiClient, TemplatesApiClient>();
            services.AddTransient<IRequestSigner>(_ => new RequestSigner(TimeProvider.System, signingKey));
 
            services.AddKeyedTransient<Func<HttpClient>>("nhs-appointments-http-client", (_, key) => 
            {
                return () =>
                {
                    var requestSigningHandler = new RequestSigningHandler(_.GetRequiredService<IRequestSigner>());
                    requestSigningHandler.InnerHandler = new HttpClientHandler();
                    var client = new HttpClient(requestSigningHandler);
                    client.BaseAddress = endpoint;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    return client;
                };
            });

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            var logger = loggerFactory.CreateLogger<NhsAppointmentsApi>();

            services.AddSingleton<ILogger>(logger);

            return services;
        }
    }
}
