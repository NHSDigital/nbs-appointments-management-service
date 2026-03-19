using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Persistance.BackoffStrategies;

namespace Nhs.Appointments.Persistance.UnitTests.BackoffStrategies;

public class BaseCosmosBackoffStrategyTests
{
    [Fact]
    public void BaseCosmosBackoffStrategy_InterfacesInspected_ImplementsICosmosBackoffStrategy()
    {
        // Assert.
        typeof(ICosmosBackoffStrategy).IsAssignableFrom(typeof(BaseCosmosBackoffStrategy))
            .Should().BeTrue();
    }

    [Fact]
    public void AfterFirstDatabaseCall_BackoffCalled_RecordsTheBackoffDetails()
    {
        // Arrange.
        var configuration = new ContainerRetryConfiguration
        {
            BackoffRetryType = BackoffRetryType.CosmosDefault,
            ContainerName = Guid.NewGuid().ToString(),
            CutoffRetryMs = 47,
            InitialValueMs = 200
        };

        var delayInMsForTestStrategy = configuration.CutoffRetryMs - 20; // Ensure it is lower than the cutoff.

        var exception = new CosmosException("Something went wrong", System.Net.HttpStatusCode.TooManyRequests, 0, "activityId", 3.7);
        var context = new CosmosBackoffContext();

        var sut = new TestBaseCosmosBackoffStrategy(configuration, new TimeSpan(0,0,0,0,delayInMsForTestStrategy));

        // Act.
        sut.Backoff(exception, context);

        // Assert.
        context.RetryCount.Should().Be(1);
        context.TotalDelayMs.Milliseconds.Should().Be(delayInMsForTestStrategy);
    }

    [Fact]
    public void AfterSecondDatabaseCallWhenCutOffWillBeExceeded_BackoffCalled_ThrowsApplicationException()
    {
        // Arrange.
        var configuration = new ContainerRetryConfiguration
        {
            BackoffRetryType = BackoffRetryType.CosmosDefault,
            ContainerName = Guid.NewGuid().ToString(),
            CutoffRetryMs = 47,
            InitialValueMs = 200
        };

        var delayInMsForTestStrategy = configuration.CutoffRetryMs - 20; // Ensure it is lower than the cutoff, but the cutoff will be exceeded when the second call is made.

        var exception = new CosmosException("Something went wrong", System.Net.HttpStatusCode.TooManyRequests, 0, "activityId", 3.7);
        var context = new CosmosBackoffContext();

        var sut = new TestBaseCosmosBackoffStrategy(configuration, new TimeSpan(0, 0, 0, 0, delayInMsForTestStrategy));
        sut.Backoff(exception, context); // After first db call.
        var expectedErrorMessage = 
                $"{context.LinkId} - Cosmos TooManyRequests failed because the CutoffRetryMs ({configuration.CutoffRetryMs}) would be exceeded on the next retry attempt : total retries: {context.RetryCount} for container: {configuration.ContainerName}, total delay time ms: {context.TotalDelayMs.TotalMilliseconds}";

        // Act.
        Action act = () => sut.Backoff(exception, context);

        // Assert.
        act.Should().Throw<ApplicationException>().WithMessage(expectedErrorMessage);
    }

    private class TestBaseCosmosBackoffStrategy(ContainerRetryConfiguration containerRetryConfiguration, TimeSpan nextRetryDelayMs) : BaseCosmosBackoffStrategy(containerRetryConfiguration) 
    {
        public override void Backoff(CosmosException ex, CosmosBackoffContext context) 
        {
            NextRetryDelayMs = nextRetryDelayMs; // Controlled by the test.

            base.Backoff(ex, context);
        }
    }
}
