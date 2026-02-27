using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Tests.Validators;

public class CancelDateRangeRequestValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly CancelDateRangeRequestValidator _sut;

    public CancelDateRangeRequestValidatorTests()
    {
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2076-12-31T00:00:00Z")));

        _sut = new CancelDateRangeRequestValidator(_timeProvider.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void FailsValidation_WhenSiteNotProvided(string site)
    {
        var request = new CancelDateRangeRequest(
            site,
            DateOnly.Parse("2077-01-01"),
            DateOnly.Parse("2077-02-01"),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDateNotProvided()
    {
        var request = new CancelDateRangeRequest(
            "test-site-123",
            new DateOnly(),
            DateOnly.Parse("2077-02-01"),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDateAfterToDate()
    {
        var request = new CancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-02-01"),
            DateOnly.Parse("2077-01-01"),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDateInThePast()
    {
        var request = new CancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2075-02-01"),
            DateOnly.Parse("2077-01-01"),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDateMoreThan90DaysInTheFuture()
    {
        var request = new CancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-05-01"),
            DateOnly.Parse("2077-05-02"),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenToDateNotProvided()
    {
        var request = new CancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-02-01"),
            new DateOnly(),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenToDateInThePast()
    {
        var request = new CancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-02-01"),
            DateOnly.Parse("2075-01-01"),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(3);
        result.Errors.Last().ErrorMessage.Should().Be("Date must be in the future.");
    }

    [Fact]
    public void FailsValidation_WhenToDateMoreThan90DaysInTheFuture()
    {
        var request = new CancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-03-01"),
            DateOnly.Parse("2077-06-01"),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenToDateMoreThan90AfterFromDate()
    {
        var request = new CancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-02-01"),
            DateOnly.Parse("2077-07-01"),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PassesValidation()
    {
        var request = new CancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-02-01"),
            DateOnly.Parse("2077-03-01"),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PassesValidation_WhenFromDateAndToDateAreTheSame()
    {
        var request = new CancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-02-01"),
            DateOnly.Parse("2077-02-01"),
            false);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
