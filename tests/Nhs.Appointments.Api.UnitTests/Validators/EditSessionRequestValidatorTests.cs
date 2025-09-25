using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;
public class EditSessionRequestValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly EditSessionRequestValidator _sut;

    public EditSessionRequestValidatorTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2024-12-31T00:00:00Z")));

        _sut = new EditSessionRequestValidator(_timeProvider.Object);
    }

    [Fact]
    public void PassesValidation_AsWildcard()
    {
        var request = new EditSessionRequest(
            "Test123",
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard
            {
                IsWildcard = true
            },
            null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PassesValidation_WithoutWildcard()
    {
        var request = new EditSessionRequest(
            "Test123",
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard
            {
                IsWildcard = false,
                Session = new Session
                {
                    From = new TimeOnly(09, 00),
                    Until = new TimeOnly(10, 00),
                    SlotLength = 5,
                    Capacity = 2,
                    Services = ["Service 1"]
                }
            },
            null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PassesValidation_WhenSessionReplacementIsNull()
    {
        var request = new EditSessionRequest(
            "Test123",
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard
            {
                IsWildcard = true
            },
            null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PassesValidation_WhenSessionReplacementIsNotNull()
    {
        var request = new EditSessionRequest(
            "Test123",
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard
            {
                IsWildcard = false,
                Session = new Session
                {
                    From = new TimeOnly(09, 00),
                    Until = new TimeOnly(10, 00),
                    SlotLength = 5,
                    Capacity = 2,
                    Services = ["Service 1"]
                }
            },
            new Session
            {
                From = new TimeOnly(09, 00),
                Until = new TimeOnly(10, 00),
                SlotLength = 5,
                Capacity = 2,
                Services = ["Service 1"]
            });

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void FailsValidation_WhenSiteNotPresent(string site)
    {
        var request = new EditSessionRequest(
            site,
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard
            {
                IsWildcard = true
            },
            null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDate_AfterToDate()
    {
        var request = new EditSessionRequest(
            "Test123",
            new DateOnly(2025, 10, 15),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard
            {
                IsWildcard = true
            },
            null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDate_InThePast()
    {
        var request = new EditSessionRequest(
            "Test123",
            new DateOnly(2023, 10, 10),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard
            {
                IsWildcard = true
            },
            null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenToDate_BeforeFromDate()
    {
        var request = new EditSessionRequest(
            "Test123",
            new DateOnly(2025, 10, 15),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard
            {
                IsWildcard = true
            },
            null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenToDate_InThePast()
    {
        var request = new EditSessionRequest(
            "Test123",
            new DateOnly(2025, 10, 15),
            new DateOnly(2023, 10, 12),
            new SessionOrWildcard
            {
                IsWildcard = true
            },
            null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}
