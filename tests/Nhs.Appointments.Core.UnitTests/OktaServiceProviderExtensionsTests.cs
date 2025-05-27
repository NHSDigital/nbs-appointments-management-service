using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core.Okta;

namespace Nhs.Appointments.Core.UnitTests;

public class OktaServiceProviderExtensionsTests
{
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    // This is a meaningless token generated with dummy data.
    // It has never been used by any of our Okta instances on any environment.
    private readonly string mockWellFormedPemToken =
        "-----BEGIN PRIVATE KEY-----\nMIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQC7VJTUt9Us8cKj\nMzEfYyjiWA4R4/M2bS1GB4t7NXp98C3SC6dVMvDuictGeurT8jNbvJZHtCSuYEvu\nNMoSfm76oqFvAp8Gy0iz5sxjZmSnXyCdPEovGhLa0VzMaQ8s+CLOyS56YyCFGeJZ\nqgtzJ6GR3eqoYSW9b9UMvkBpZODSctWSNGj3P7jRFDO5VoTwCQAWbFnOjDfH5Ulg\np2PKSQnSJP3AJLQNFNe7br1XbrhV//eO+t51mIpGSDCUv3E0DDFcWDTH9cXDTTlR\nZVEiR2BwpZOOkE/Z0/BVnhZYL71oZV34bKfWjQIt6V/isSMahdsAASACp4ZTGtwi\nVuNd9tybAgMBAAECggEBAKTmjaS6tkK8BlPXClTQ2vpz/N6uxDeS35mXpqasqskV\nlaAidgg/sWqpjXDbXr93otIMLlWsM+X0CqMDgSXKejLS2jx4GDjI1ZTXg++0AMJ8\nsJ74pWzVDOfmCEQ/7wXs3+cbnXhKriO8Z036q92Qc1+N87SI38nkGa0ABH9CN83H\nmQqt4fB7UdHzuIRe/me2PGhIq5ZBzj6h3BpoPGzEP+x3l9YmK8t/1cN0pqI+dQwY\ndgfGjackLu/2qH80MCF7IyQaseZUOJyKrCLtSD/Iixv/hzDEUPfOCjFDgTpzf3cw\nta8+oE4wHCo1iI1/4TlPkwmXx4qSXtmw4aQPz7IDQvECgYEA8KNThCO2gsC2I9PQ\nDM/8Cw0O983WCDY+oi+7JPiNAJwv5DYBqEZB1QYdj06YD16XlC/HAZMsMku1na2T\nN0driwenQQWzoev3g2S7gRDoS/FCJSI3jJ+kjgtaA7Qmzlgk1TxODN+G1H91HW7t\n0l7VnL27IWyYo2qRRK3jzxqUiPUCgYEAx0oQs2reBQGMVZnApD1jeq7n4MvNLcPv\nt8b/eU9iUv6Y4Mj0Suo/AU8lYZXm8ubbqAlwz2VSVunD2tOplHyMUrtCtObAfVDU\nAhCndKaA9gApgfb3xw1IKbuQ1u4IF1FJl3VtumfQn//LiH1B3rXhcdyo3/vIttEk\n48RakUKClU8CgYEAzV7W3COOlDDcQd935DdtKBFRAPRPAlspQUnzMi5eSHMD/ISL\nDY5IiQHbIH83D4bvXq0X7qQoSBSNP7Dvv3HYuqMhf0DaegrlBuJllFVVq9qPVRnK\nxt1Il2HgxOBvbhOT+9in1BzA+YJ99UzC85O0Qz06A+CmtHEy4aZ2kj5hHjECgYEA\nmNS4+A8Fkss8Js1RieK2LniBxMgmYml3pfVLKGnzmng7H2+cwPLhPIzIuwytXywh\n2bzbsYEfYx3EoEVgMEpPhoarQnYPukrJO4gwE2o5Te6T5mJSZGlQJQj9q4ZB2Dfz\net6INsK0oG8XVGXSpQvQh3RUYekCZQkBBFcpqWpbIEsCgYAnM3DQf3FJoSnXaMhr\nVBIovic5l0xFkEHskAjFTevO86Fsz1C2aSeRKSqGFoOQ0tmJzBEs1R6KqnHInicD\nTQrKhArgLXX4v3CddjfTRJkFWDbE/CkvKZNOrcf1nhaGCPspRJj2KUkj1Fhl9Cnc\ndn/RsYEONbwQSjIfMPkvxF+8HQ==\n-----END PRIVATE KEY-----";

    [Fact(DisplayName = "Registers the Okta store if Okta config is valid")]
    public void OktaRegistration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Okta:Domain", "https://local.okta.com" },
                { "Okta:ManagementId", "1234abc" },
                { "Okta:PrivateKeyKid", "1234abc" },
                { "Okta:PEM", mockWellFormedPemToken }
            })
            .Build();

        var services = _serviceCollection.AddLogging().AddOktaUserDirectory(configuration);


        var serviceProvider = services.BuildServiceProvider();
        var oktaService = serviceProvider.GetService(typeof(IOktaUserDirectory));

        oktaService.Should().BeOfType<OktaUserDirectory>();
    }

    [Fact(DisplayName = "Registers a local stub of the Okta store if Okta config is local")]
    public void OktaRegistration_Local()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Okta:Domain", "https://local.okta.com" },
                { "Okta:ManagementId", "local" },
                { "Okta:PrivateKeyKid", "local" },
                { "Okta:PEM", "local" }
            })
            .Build();

        var services = _serviceCollection.AddLogging().AddOktaUserDirectory(configuration);


        var serviceProvider = services.BuildServiceProvider();
        var oktaService = serviceProvider.GetService(typeof(IOktaUserDirectory));

        oktaService.Should().BeOfType<OktaLocalUserDirectory>();
    }

    [Fact(DisplayName = "Registers an unimplemented Okta store if Okta config is missing")]
    public void OktaRegistration_Unimplemented()
    {
        var configuration = new ConfigurationBuilder().Build();

        var services = _serviceCollection.AddLogging().AddOktaUserDirectory(configuration);


        var serviceProvider = services.BuildServiceProvider();
        var oktaService = serviceProvider.GetService(typeof(IOktaUserDirectory));

        oktaService.Should().BeOfType<OktaUnimplementedUserDirectory>();
    }
}
