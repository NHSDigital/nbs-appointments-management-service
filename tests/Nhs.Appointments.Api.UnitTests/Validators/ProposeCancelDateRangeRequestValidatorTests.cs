using FluentAssertions;
using Moq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class ProposeCancelDateRangeRequestValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly ProposeCancelDateRangeRequestValidator _sut;

    public ProposeCancelDateRangeRequestValidatorTests()
    {
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2076-12-31T00:00:00Z")));

        _sut = new ProposeCancelDateRangeRequestValidator(_timeProvider.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void FailsValidation_WhenSiteNotProvided(string site)
    {
        var request = new ProposeCancelDateRangeRequest
        (
            site,
            DateOnly.Parse("2077-01-01"),
            DateOnly.Parse("2077-01-02")
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Site" && e.ErrorMessage == "Site is required.");
    }

    [Fact]
    public void FailsValidation_WhenFromDateNotProvided()
    {
        var request = new ProposeCancelDateRangeRequest
        (
            "SiteA",
            new DateOnly(),
            DateOnly.Parse("2077-01-02")
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDateInPast()
    {
        var request = new ProposeCancelDateRangeRequest
        (
            "SiteA",
            DateOnly.Parse("2076-12-30"),
            DateOnly.Parse("2077-01-02")
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDateAfterToDate()
    {
        var request = new ProposeCancelDateRangeRequest
        (
            "SiteA",
            DateOnly.Parse("2077-01-03"),
            DateOnly.Parse("2077-01-02")
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenFromDateMoreThan3MonthsInFuture()
    {
        var request = new ProposeCancelDateRangeRequest
        (
            "SiteA",
            DateOnly.Parse("2077-04-01"),
            DateOnly.Parse("2077-04-02")
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenToDateNotProvided()
    {
        var request = new ProposeCancelDateRangeRequest
        (
            "SiteA",
            DateOnly.Parse("2077-01-01"),
            new DateOnly()
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenToDateInPast()
    {
        var request = new ProposeCancelDateRangeRequest
        (
            "SiteA",
            DateOnly.Parse("2077-01-01"),
            DateOnly.Parse("2076-12-30")
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void FailsValidation_WhenToDateMoreThan3MonthsInFuture()
    {
        var request = new ProposeCancelDateRangeRequest
        (
            "SiteA",
            DateOnly.Parse("2077-01-01"),
            DateOnly.Parse("2077-04-01")
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PassesValidation()
    {
        var request = new ProposeCancelDateRangeRequest
        (
            "SiteA",
            DateOnly.Parse("2077-01-01"),
            DateOnly.Parse("2077-01-02")
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
