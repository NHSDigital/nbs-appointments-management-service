using System.Net;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Persistance.BackoffStrategies;
using Nhs.Appointments.Persistance.UnitTests.Helpers;

namespace Nhs.Appointments.Persistance.UnitTests.BackoffStrategies;

public class CosmosDefaultBackoffStrategyTests
{
    [Fact]
    public void SutIsCreated_BackoffIsCalled_NextRetryDelayMsShouldBeExceptionsRetryValue()
    {
        // Arrange.
        var retryConfiguration = new ContainerRetryConfiguration();
        var expectedRetryValue = new TimeSpan(0, 0, 0, 0, 100);
        var exception = new RetryAfterCosmosException(expectedRetryValue);
        var context = new CosmosBackoffContext();

        var sut = new CosmosDefaultBackoffStrategy(retryConfiguration);

        // Act.
        sut.Backoff(exception, context);

        // Assert.
        sut.NextRetryDelayMs.Should().Be(expectedRetryValue);
    }

    [Fact]
    public void ExceptionIsThrownWithNoRetryValue_BackoffIsCalled_ThrowsInvalidOperationException()
    {
        // Arrange.
        var retryConfiguration = new ContainerRetryConfiguration();
        var expectedRetryValue = new TimeSpan(0, 0, 0, 0, 100);
        var exception = new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, Guid.NewGuid().ToString(), 2);
        var context = new CosmosBackoffContext();

        var sut = new CosmosDefaultBackoffStrategy(retryConfiguration);

        // Act.
        Action act = () => sut.Backoff(exception, context);

        // Assert.
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("TooManyRequests exception does not have a RetryAfter value");
    }

    [Fact]
    public void MaxNumberOfRetriesIsReached_BackoffIsCalled_ThrowsBackoffException()
    {
        // Arrange.
        var containerName = Guid.NewGuid().ToString();
        var retryConfiguration = new ContainerRetryConfiguration { ContainerName = containerName };
        var delayValue = new TimeSpan(0, 0, 0, 0, 100);
        var exception = new RetryAfterCosmosException(delayValue);
        var context = new CosmosBackoffContext();
        for (var i = 0; i < CosmosDefaultBackoffStrategy.DefaultCosmosMaxRetries; i++)
        {
            context.RecordBackoff(delayValue);
        }
        var expectedError = 
                $"{context.LinkId} - Cosmos TooManyRequests failed after max retries ({CosmosDefaultBackoffStrategy.DefaultCosmosMaxRetries}) exceeded for container: {retryConfiguration.ContainerName}, total delay time ms: {context.TotalDelayMs.TotalMilliseconds}";

        var sut = new CosmosDefaultBackoffStrategy(retryConfiguration);

        // Act.
        Action act = () => sut.Backoff(exception, context);

        // Assert.
        act.Should()
            .Throw<BackoffException>()
            .WithMessage(expectedError);
    }
}
