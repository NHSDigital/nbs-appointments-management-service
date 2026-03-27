using FluentAssertions;
using Nhs.Appointments.Persistance.BackoffStrategies;

namespace Nhs.Appointments.Persistance.UnitTests;

public class CosmosOperationTimingsTests
{
    [Fact]
    public void StartTimeIsNotSet_StartTimeIsSet_StartTimeIsSetCorrectly()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var sut = new CosmosOperationAttemptTiming();

        // Act.
        Action act = () => sut.StartTime = fixedTime;

        act.Should().NotThrow<Exception>();
        sut.StartTime.Should().Be(fixedTime);
    }

    [Fact]
    public void StartTimeIsAlreadySet_StartTimeIsSetAgain_ThrowsInvalidOperationException()
    {
        // Arrange.
        var sut = new CosmosOperationAttemptTiming();
        sut.StartTime = DateTime.UtcNow;

        // Act.
        Action act = () => sut.StartTime = DateTime.UtcNow;

        // Assert.
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("StartTime cannot be set more than once.");
    }

    [Fact]
    public void StartTimeIsNotSet_EndTimeIsSet_ThrowsInvalidOperationException()
    {
        // Arrange.
        var sut = new CosmosOperationAttemptTiming();

        // Act.
        Action act = () => sut.EndTime = DateTime.UtcNow;

        // Assert.
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("EndTime cannot be set until the StartTime has been set.");
    }

    [Fact]
    public void StartTimeIsSet_EndTimeIsSet_EndTimeIsSetCorrectly()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var startTime = fixedTime;

        var sut = new CosmosOperationAttemptTiming();
        sut.StartTime = startTime;

        // Act.
        Action act = () => sut.EndTime = fixedTime;

        act.Should().NotThrow<Exception>();
        sut.EndTime.Should().Be(fixedTime);
    }


    [Fact]
    public void StartTimeIsSet_EndTimeIsSetEarlierThanStartTime_ThrowsInvalidOperationException()
    {
        // Arrange.
        var sut = new CosmosOperationAttemptTiming();
        var fixedTime = DateTime.UtcNow;
        sut.StartTime = fixedTime;

        // Act.
        Action act = () => sut.EndTime = fixedTime.AddMilliseconds(-1);

        // Assert.
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("EndTime cannot be set to an earlier value than the StartTime.");
    }

    [Fact]
    public void BothStartTimeAndEndTimeAreSet_EndTimeIsSetAgain_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var startTime = fixedTime;
        var endTime = startTime.AddMilliseconds(1);

        var sut = new CosmosOperationAttemptTiming();
        sut.StartTime = startTime;
        sut.EndTime = endTime;

        // Act.
        Action act = () => sut.EndTime = endTime.AddMilliseconds(1);   

        // Assert.
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("EndTime cannot be set more than once.");
    }

    [Fact]
    public void StartTimeOnlyIsSet_EndTimeIsRead_EndTimeIsNull()
    {
        // Arrange.
        var sut = new CosmosOperationAttemptTiming();
        sut.StartTime = DateTime.UtcNow;

        // Act - not required.

        // Assert.
        sut.EndTime.Should().BeNull();
    }

    [Fact]
    public void StartTimeOnlyIsSet_ElapsedIsRead_ElapsedIsNull()
    {
        // Arrange.
        var sut = new CosmosOperationAttemptTiming();
        sut.StartTime = DateTime.UtcNow;

        // Act - not required.

        // Assert.
        sut.Elapsed.Should().BeNull();
    }

    [Fact]
    public void BothStartTimeAndEndTimeAreSet_ElapsedIsRead_ElapsedIsCalculatedCorrectly()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var expectedElapsedMilliseconds = 116;
        var startTime = fixedTime;

        var sut = new CosmosOperationAttemptTiming();
        sut.StartTime = startTime;
        sut.EndTime = startTime.AddMilliseconds(expectedElapsedMilliseconds);

        // Act - not required.

        // Assert.
        sut.Elapsed.Should().Be(TimeSpan.FromMilliseconds(expectedElapsedMilliseconds));
    }
}
