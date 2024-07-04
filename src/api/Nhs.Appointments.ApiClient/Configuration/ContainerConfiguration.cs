using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Nhs.Appointments.ApiClient.Auth;
using Nhs.Appointments.ApiClient.Impl;

namespace Nhs.Appointments.ApiClient.Configuration
{
    public static class ContainerConfiguration
    {
        internal const string HttpClientKey = "nhs-appointments-http-client";
        public static IServiceCollection AddNhsAppointmentsApiClient(this IServiceCollection services, Uri endpoint, string signingKey)
        {
            services.AddTransient<INhsAppointmentsApi, NhsAppointmentsApi>();
            services.AddTransient<IBookingsApiClient, BookingsApiClient>();
            services.AddTransient<ISitesApiClient, SitesApiClient>();
            services.AddTransient<ITemplatesApiClient, TemplatesApiClient>();
            services.AddTransient<IRequestSigner>(_ => new RequestSigner(TimeProvider.System, signingKey));
 
            services.AddKeyedTransient<Func<HttpClient>>(HttpClientKey, (_, key) => 
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

            services.AddLogging();

            return services;
        }
    }
}
