using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.ApiClient;
using Nhs.Appointments.ApiClient.Configuration;

namespace Nhs.Appointments.Api.Client.UnitTests
{
    public class ConfigurationTests
    {
        [Fact]
        public void CanConfigureClientForDI()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddNhsAppointmentsApiClient(new Uri("http://localhost/"), "my_client_id", "my_signing_key");
            var container = services.BuildServiceProvider();
            var client = container.GetRequiredService<INhsAppointmentsApi>();
            Assert.NotNull(client);
            Assert.NotNull(client.Bookings);
            Assert.NotNull(client.Sites);
            Assert.NotNull(client.Templates);
        }

        /// <summary>
        /// Users of this library will probably add common .NET types to their container; we need to make sure they are unaffected by our own service registrations
        /// </summary>
        [Fact]
        public void LocalDIConfigurationIsUnaffected()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddNhsAppointmentsApiClient(new Uri("http://localhost/"), "my_client_id", "my_signing_key");
            services.AddTransient(_ => new HttpClient { BaseAddress = new Uri("http://tempuri.org/") });
            var container = services.BuildServiceProvider();
            var theirClient = container.GetRequiredService<HttpClient>();
            Assert.Equal("http://tempuri.org/", theirClient.BaseAddress.ToString());
        }
    }
}