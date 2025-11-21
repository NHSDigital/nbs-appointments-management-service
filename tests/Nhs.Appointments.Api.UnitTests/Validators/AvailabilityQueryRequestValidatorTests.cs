using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;
public class AvailabilityQueryRequestValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly AvailabilityQueryRequestValidator _sut;

    public AvailabilityQueryRequestValidatorTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2025-09-01T00:00:00Z")));

        _sut = new AvailabilityQueryRequestValidator(_timeProvider.Object);
    }

    [Fact]
    public void FailsValidation_WhenSitesListIsEmpty()
    {
        var request = new AvailabilityQueryRequest(
            [],
            [
                new() { Services = ["RSV:Adult"] }
            ],
            new DateOnly(2025, 9, 1),
            new DateOnly(2025, 10, 1));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenAttendeesListIsEmpty()
    {
        var request = new AvailabilityQueryRequest(
            ["test-site-123"],
            [],
            new DateOnly(2025, 9, 1),
            new DateOnly(2025, 10, 1));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromNotProvided()
    {
        var request = new AvailabilityQueryRequest(
            ["test-site-123"],
            [
                new() { Services = ["RSV:Adult"] }
            ],
            default,
            new DateOnly(2025, 10, 1));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenUntilNotProvided()
    {
        var request = new AvailabilityQueryRequest(
            ["test-site-123"],
            [
                new() { Services = ["RSV:Adult"] }
            ],
            new DateOnly(2025, 10, 1),
            default);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromAfterUntil()
    {
        var request = new AvailabilityQueryRequest(
            ["test-site-123"],
            [
                new() { Services = ["RSV:Adult"] }
            ],
            new DateOnly(2025, 10, 1),
            new DateOnly(2025, 9, 1));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PassValidation()
    {
        var request = new AvailabilityQueryRequest(
            ["test-site-123"],
            [
                new() { Services = ["RSV:Adult"] }
            ],
            new DateOnly(2025, 9, 2),
            new DateOnly(2025, 10, 1));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
