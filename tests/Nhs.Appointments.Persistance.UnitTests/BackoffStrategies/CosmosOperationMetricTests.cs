using FluentAssertions;

namespace Nhs.Appointments.Persistance.UnitTests.BackoffStrategies;

public class CosmosOperationMetricTests
{
    [Fact]
    public void MetricIsCreated_NameIsRead_NameIsCorrect()
    {
        // Arrange.
        var sut = new CosmosOperationMetric();

        // Act / Assert.
        sut.Name.Should().Be("CosmosOperationMetric");
    }

    [Fact]
    public void MetricIsCreatedWithContainerName_ContainerNameIsRead_ContainerNameIsCorrect()
    {
        // Arrange.
        var containerName = Guid.NewGuid().ToString();
        var sut = new CosmosOperationMetric { Container = containerName };

        // Act / Assert.
        sut.Container.Should().Be(containerName);
    }

    [Fact]
    public void MetricIsCreatedWithDocumentType_DocumentTypeIsRead_DocumentTypeIsCorrect()
    {
        // Arrange.
        var documentType = Guid.NewGuid().ToString();
        var sut = new CosmosOperationMetric { DocumentType = documentType };

        // Act / Assert.
        sut.DocumentType.Should().Be(documentType);
    }

    [Fact]
    public void MetricIsCreated_StartAttemptIsCalled_StartTimeIsRecordedAndEndTimeIsNull()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var startTime = fixedTime;

        var sut = new CosmosOperationMetric();

        // Act.
        sut.StartAttempt(startTime);

        // Assert.
        sut.StartTime.Should().Be(startTime);
        sut.EndTime.Should().BeNull();
        sut.Timings.Should().HaveCount(1);
        sut.Timings.First().StartTime.Should().Be(startTime);
        sut.Timings.First().EndTime.Should().BeNull();
    }

    [Fact]
    public void StartAttemptHasBeenCalled_EndAttemptIsCalled_EndTimeIsRecorded()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var startTime = fixedTime;
        var endTime = fixedTime.AddMilliseconds(123);

        var sut = new CosmosOperationMetric();
        sut.StartAttempt(startTime);

        // Act.
        sut.EndAttempt(endTime);

        // Assert.
        sut.StartTime.Should().Be(startTime);
        sut.EndTime.Should().Be(endTime);
        sut.Timings.Should().HaveCount(1);
        sut.Timings.First().StartTime.Should().Be(startTime);
        sut.Timings.First().EndTime.Should().Be(endTime);
    }

    [Fact]
    public void StartAttemptHasNotBeenCalled_EndAttemptIsCalled_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var sut = new CosmosOperationMetric();
        var endTime = fixedTime.AddMilliseconds(123);

        // Act.
        Action act = () => sut.EndAttempt(endTime);

        // Assert.
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("EndAttempt cannot be called before StartAttempt");
    }

    [Fact]
    public void StartAttemptAndEndAttemptHaveAlreadyBeenCalled_StartAttemptIsCalled_StartTimeIsRecordedAndEndTimeIsNull()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var startTime = fixedTime;
        var sut = new CosmosOperationMetric();
        var endTime = startTime.AddMilliseconds(123);
        var nextStartTime = endTime.AddMilliseconds(456);
        sut.StartAttempt(startTime);
        sut.EndAttempt(endTime);

        // Act.
        sut.StartAttempt(nextStartTime);

        // Assert.
        sut.StartTime.Should().Be(startTime);
        sut.EndTime.Should().BeNull();
        sut.Timings.Should().HaveCount(2);
        sut.Timings.First().StartTime.Should().Be(startTime);
        sut.Timings.First().EndTime.Should().Be(endTime);
        sut.Timings[1].StartTime.Should().Be(nextStartTime);
        sut.Timings[1].EndTime.Should().BeNull();
    }

    [Fact]
    public void StartAttemptEndAttemptAndStartAttemptHaveAlreadyBeenCalled_EndAttemptIsCalled_EndTimeIsRecorded()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var startTime = fixedTime;
        var sut = new CosmosOperationMetric();
        var firstEndTime = fixedTime.AddMilliseconds(123);
        var nextStartTime = firstEndTime.AddMilliseconds(234);
        var nextEndTime = nextStartTime.AddMilliseconds(345);
        sut.StartAttempt(startTime);
        sut.EndAttempt(firstEndTime);
        sut.StartAttempt(nextStartTime);

        // Act.
        sut.EndAttempt(nextEndTime);

        // Assert.
        sut.StartTime.Should().Be(startTime);
        sut.EndTime.Should().Be(nextEndTime);
        sut.Timings.Should().HaveCount(2);
        sut.Timings.First().StartTime.Should().Be(startTime);
        sut.Timings.First().EndTime.Should().Be(firstEndTime);
        sut.Timings[1].StartTime.Should().Be(nextStartTime);
        sut.Timings[1].EndTime.Should().Be(nextEndTime);
    }

    [Fact]
    public void StartAttemptHasBeenCalled_StartAttemptIsCalledAgain_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var startTime = fixedTime;
        var sut = new CosmosOperationMetric();
        var nextStartTime = startTime.AddMilliseconds(234);
        sut.StartAttempt(startTime);

        // Act.
        Action act = () => sut.StartAttempt(nextStartTime);

        // Assert.
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("StartAttempt cannot be called before EndAttempt");
    }

    [Fact]
    public void StartAttemptAndEndAttemptHaveAlreadyBeenCalled_EndAttemptIsCalled_ThrowsInvalidOperationException()
    {
        // Arrange.
        var fixedTime = DateTime.UtcNow;
        var startTime = fixedTime;
        var sut = new CosmosOperationMetric();
        var firstEndTime = fixedTime.AddMilliseconds(123);
        var nextStartTime = firstEndTime.AddMilliseconds(234);
        var nextEndTime = nextStartTime.AddMilliseconds(345);
        sut.StartAttempt(startTime);
        sut.EndAttempt(firstEndTime);

        // Act.
        Action act = () => sut.EndAttempt(nextEndTime);

        // Assert.
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("EndAttempt cannot be called before StartAttempt");
    }

    [Fact]
    public void MetricIsCreated_RuChargeIsRead_ReturnsZero()
    {
        // Arrange.
        var sut = new CosmosOperationMetric();

        // Act - not required.

        // Assert.
        sut.RuCharge.Should().Be(0);
    }

    [Theory]
    [InlineData(3.61)]
    [InlineData(5.12)]
    [InlineData(6.4)]
    [InlineData(6)]
    public void MetricIsCreated_AddRuChargeIsCalled_ReturnsValue(double ruValue)
    {
        // Arrange.
        var sut = new CosmosOperationMetric();

        // Act.
        sut.AddRuCharge(ruValue);

        // Assert.
        sut.RuCharge.Should().Be(ruValue);
    }

    [Fact]
    public void MetricIsCreated_AddRuChargeIsCalledWithNegativeValue_ThrowsArgumentException()
    {
        // Arrange.
        var ruValue = -3.61;
        var sut = new CosmosOperationMetric();

        // Act.
        Action act = () => sut.AddRuCharge(ruValue);

        // Assert.
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MetricIsCreated_AddRuChargeIsCalledMultipleTimes_ReturnsTotalValue()
    {
        // Arrange.
        var sut = new CosmosOperationMetric();
        double[] ruCharges = { 1.23, 4.56, 7.89, 3.45, 8, 2.01 };
        var total = ruCharges.Sum();

        // Act.
        foreach (var ruCharge in ruCharges)
        {
            sut.AddRuCharge(ruCharge);
        }

        // Assert.
        sut.RuCharge.Should().Be(total);
    }
}
