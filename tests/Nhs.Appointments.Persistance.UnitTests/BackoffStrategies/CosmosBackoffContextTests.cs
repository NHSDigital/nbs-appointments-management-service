using FluentAssertions;
using Nhs.Appointments.Persistance.BackoffStrategies;

namespace Nhs.Appointments.Persistance.UnitTests.BackoffStrategies;

public class CosmosBackoffContextTests
{
    [Fact]
    public void SutIsCreated_InitialPropertyValuesAreInspected_InitialPropertyValuesAreCorrect()
    {
        // Arrange - not required.

        // Act.
        var sut = new CosmosBackoffContext();

        // Assert.
        sut.LinkId.Should().NotBeEmpty();
        sut.RetryCount.Should().Be(0);
        sut.TotalDelayMs.Should().Be(TimeSpan.FromMilliseconds(0));
        sut.IsReattempt.Should().BeFalse();
    }

    [Fact]
    public void SutIsCreated_RecordBackoffIsCalledOnce_StateIsCorrect()
    {
        // Arrange.
        var sut = new CosmosBackoffContext();
        var linkId = sut.LinkId;
        var randomDelay = new Random().Next(100, 2000);
        var expectedDelay = new TimeSpan(randomDelay);

        // Act.
        sut.RecordBackoff(expectedDelay);

        // Assert.
        sut.LinkId.Should().Be(linkId);
        sut.TotalDelayMs.Should().Be(expectedDelay);
        sut.RetryCount.Should().Be(1);
        sut.IsReattempt.Should().BeTrue();
    }

    [Fact]
    public void SutIsCreated_RecordBackoffIsCalledTwice_StateIsCorrect()
    {
        // Arrange.
        var sut = new CosmosBackoffContext();
        var linkId = sut.LinkId;
        var random = new Random();
        var initialRandomDelay = random.Next(100, 2000);
        var initialDelay = new TimeSpan(initialRandomDelay);
        var subsequentRandomDelay = random.Next(100, 2000);
        var subsequentDelay = new TimeSpan(subsequentRandomDelay);

        // Act.
        sut.RecordBackoff(initialDelay);
        sut.RecordBackoff(subsequentDelay);

        // Assert.
        sut.LinkId.Should().Be(linkId);
        sut.TotalDelayMs.Should().Be(initialDelay + subsequentDelay);
        sut.RetryCount.Should().Be(2);
        sut.IsReattempt.Should().BeTrue();
    }
}
