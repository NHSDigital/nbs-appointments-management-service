using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Api.Tests.Validators;
public class AvailabilityFilterValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly AvailabilityFilterValidator _sut;

    public AvailabilityFilterValidatorTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2025-09-01T00:00:00Z")));

        _sut = new AvailabilityFilterValidator(_timeProvider.Object);
    }

    [Fact]
    public void FailsValidation_WhenOnlyOneServiceFilterIsProvided()
    {
        var filter = new AvailabilityFilter
        {
            Services = ["test_service"],
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenOnlyFromTimeIsProvided()
    {
        var filter = new AvailabilityFilter
        {
            From = new DateOnly(2025, 9, 10),
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenOnlyUntilTimeIsProvided()
    {
        var filter = new AvailabilityFilter
        {
            Until = new DateOnly(2024, 10, 1)
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromServiceFilterIsInThePast()
    {
        var filter = new AvailabilityFilter
        {
            Services = ["test_service"],
            From = new DateOnly(2025, 9, 10),
            Until = new DateOnly(2024, 10, 1)
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenUntilServiceFilterIsInThePast()
    {
        var filter = new AvailabilityFilter
        {
            Services = ["test_service"],
            From = new DateOnly(2025, 10, 10),
            Until = new DateOnly(2025, 10, 1)
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }


    [Fact]
    public void FailsValidation_WhenUntilFilterIsBeforeFromFilter()
    {
        var filter = new AvailabilityFilter
        {
            Services = ["test_service"],
            From = new DateOnly(2025, 9, 2),
            Until = new DateOnly(2025, 8, 1)
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PassesValidation()
    {
        var filter = new AvailabilityFilter
        {
            Services = ["RSV:Adult"],
            From = new DateOnly(2025, 9, 2),
            Until = new DateOnly(2025, 9, 15),
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PassesValidation_WithMultipleServices()
    {
        var filter = new AvailabilityFilter
        {
            Services = ["RSV:Adult", "COVID:5_11"],
            From = new DateOnly(2025, 9, 2),
            Until = new DateOnly(2025, 9, 15),
        };

        var result = _sut.Validate(filter);

        result.IsValid.Should().BeTrue();
    }
}
