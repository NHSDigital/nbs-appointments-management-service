using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;
public class CancelSessionReqestValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly CancelSessionRequestValidator _sut;

    public CancelSessionReqestValidatorTests()
    {
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2024-12-31T00:00:00Z")));

        _sut = new CancelSessionRequestValidator(_timeProvider.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenSiteNotSupplied(string site)
    {
        var request = new CancelSessionRequest(
            site,
            new DateOnly(2025, 1, 10),
            "09:00",
            "12:00",
            ["RSV:Adult"],
            5, 2);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
    }

    [Fact]
    public void Validate_ReturnsError_WhenServicesNotSupplied()
    {
        var request = new CancelSessionRequest(
            "TEST01",
            new DateOnly(2025, 1, 10),
            "09:00",
            "12:00",
            null,
            5, 2);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenFromNotSupplied(string from)
    {
        var request = new CancelSessionRequest(
            "TEST01",
            new DateOnly(2025, 1, 10),
            from,
            "12:00",
            ["RSV:Adult"],
            5, 2);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenUntilNotSupplied(string until)
    {
        var request = new CancelSessionRequest(
            "TEST01",
            new DateOnly(2025, 1, 10),
            "09:00",
            until,
            ["RSV:Adult"],
            5, 2);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
    }

    [Fact]
    public void Validate_ReturnsError_WhenSlotLengthIsZero()
    {
        var request = new CancelSessionRequest(
            "TEST01",
            new DateOnly(2025, 1, 10),
            "09:00",
            "12:00",
            ["RSV:Adult"],
            0, 2);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
    }

    [Fact]
    public void Validate_ReturnsError_WhenCapacityIsZero()
    {
        var request = new CancelSessionRequest(
            "TEST01",
            new DateOnly(2025, 1, 10),
            "09:00",
            "12:00",
            ["RSV:Adult"],
            5, 0);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var request = new CancelSessionRequest(
            "TEST01",
            new DateOnly(2025, 1, 10),
            "09:00",
            "12:00",
            ["RSV:Adult"],
            5, 2);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
