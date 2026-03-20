using System.Net;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Persistance.BackoffStrategies;
using Nhs.Appointments.Persistance.UnitTests.Helpers;

namespace Nhs.Appointments.Persistance.UnitTests.BackoffStrategies;

public class CosmosLinearBackoffStrategyTests
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

        var sut = new CosmosLinearBackoffStrategy(retryConfiguration);

        // Act.
        sut.Backoff(exception, context);

        // Assert.
        sut.NextRetryDelayMs.Should().Be(TimeSpan.FromMilliseconds(randomInitialValueMs));
    }

    [Fact]
    public void SutIsCreated_BackoffIsCalledRepeatedly_NextRetryDelayMsShouldBeInitialValueFromRetryConfiguration()
    {
        // Arrange.
        var randomInitialValueMs = Random.Next(1000, 5000);
        var randomNumberOfBackoffCalls = Random.Next(5, 10);

        var retryConfiguration = new ContainerRetryConfiguration { InitialValueMs = randomInitialValueMs };
        var exception = new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, Guid.NewGuid().ToString(), 2);
        var context = new CosmosBackoffContext();

        var sut = new CosmosLinearBackoffStrategy(retryConfiguration);

        // Act.
        for (var i = 0; i < randomNumberOfBackoffCalls; i++)
        {
            sut.Backoff(exception, context);
        }

        // Assert.
        sut.NextRetryDelayMs.Should().Be(TimeSpan.FromMilliseconds(randomInitialValueMs));
    }
}
