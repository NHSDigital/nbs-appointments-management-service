using System.Net;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Persistance.BackoffStrategies;

namespace Nhs.Appointments.Persistance.UnitTests.BackoffStrategies;

public class CosmosExponentialBackoffStrategyTests
{
    private static readonly Random Random = new();

    [Fact]
    public void SutIsCreated_BackoffIsCalled_NextRetryDelayMsShouldBeInitialValueFromRetryConfiguration()
    {
        // Arrange.
        var randomInitialValueMs = Random.Next(1000, 5000);

        var retryConfiguration = new ContainerRetryConfiguration { InitialValueMs = randomInitialValueMs };
        var exception = new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, Guid.NewGuid().ToString(), 2);
        var context = new CosmosBackoffContext();

        var sut = new CosmosExponentialBackoffStrategy(retryConfiguration);

        // Act.
        sut.Backoff(exception, context);

        // Assert.
        sut.NextRetryDelayMs.Should().Be(TimeSpan.FromMilliseconds(randomInitialValueMs));
    }

    [Fact]
    public void SutIsCreated_BackoffIsCalledMultipleTimes_NextRetryDelayMsShouldBeInitialValueFromRetryConfigurationRaisedToTheRetryCountPower()
    {
        // Arrange.
        var randomInitialValueMs = Random.Next(1000, 5000);
        const int numberOfBackoffs = 10;

        var numberOfRetries = numberOfBackoffs - 1;
        var retryConfiguration = new ContainerRetryConfiguration { InitialValueMs = randomInitialValueMs };
        var exception = new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, Guid.NewGuid().ToString(), 2);
        var context = new CosmosBackoffContext();
        var sut = new CosmosExponentialBackoffStrategy(retryConfiguration);

        // Act.
        for (var i = 0; i < numberOfRetries; i++)
        {
            sut.Backoff(exception, context);
            
            // Assert.
            var exponent = Math.Log(retryConfiguration.InitialValueMs) + i;
            sut.NextRetryDelayMs.Should().Be(TimeSpan.FromMilliseconds((int)Math.Floor(Math.Exp(exponent))));
        }
    }
}
