using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;

namespace Nhs.Appointments.Api.Tests.Validators;

public class ProposeCancelDateRangeRequestValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly Mock<IOptions<ChangeAvailabilityOptions>> _availabilityConfig = new();
    private readonly ProposeCancelDateRangeRequestValidator _sut;
    
    public ProposeCancelDateRangeRequestValidatorTests()
    {
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2075-12-31T00:00:00Z")));
        _availabilityConfig.Setup(x => x.Value).Returns(new ChangeAvailabilityOptions
        {
            CancelADateRangeMaximumDays = 90
        });

        _sut = new ProposeCancelDateRangeRequestValidator(
            _timeProvider.Object,
            _availabilityConfig.Object
        );
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
            DateOnly.Parse("2075-12-30"),
            DateOnly.Parse("2076-01-02")
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
    public void PassesValidation_WhenToDate89DaysAfterFromDate_DSTCrossed_1()
    {
        var request = new ProposeCancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-01-15"),
            DateOnly.Parse("2077-04-14"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void PassesValidation_WhenToDate90DaysAfterFromDate_DSTCrossed_1()
    {
        var request = new ProposeCancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-01-14"),
            DateOnly.Parse("2077-04-14"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void PassesValidation_WhenToDate89DaysAfterFromDate_DSTCrossed_2()
    {
        var request = new ProposeCancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-09-01"),
            DateOnly.Parse("2077-11-29"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void PassesValidation_WhenToDate90DaysAfterFromDate_DSTCrossed_2()
    {
        var request = new ProposeCancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2077-09-01"),
            DateOnly.Parse("2077-11-30"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public void PassesValidation_WhenToDate89DaysAfterFromDate_DSTCrossed_LeapYear_1()
    {
        var request = new ProposeCancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2076-01-16"),
            DateOnly.Parse("2076-04-14"));

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void PassesValidation_WhenToDate90DaysAfterFromDate_DSTCrossed_LeapYear_2()
    {
        var request = new ProposeCancelDateRangeRequest(
            "test-site-123",
            DateOnly.Parse("2076-01-15"),
            DateOnly.Parse("2076-04-14"));

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

    [Fact]
    public void FailsValidation_WhenFromGreaterThan3MonthsAfterTo()
    {
        var request = new ProposeCancelDateRangeRequest
        (
            "SiteA",
            DateOnly.Parse("2077-01-01"),
            DateOnly.Parse("2077-05-02")
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(91, 90, false)]
    [InlineData(90, 90, false)]
    [InlineData(89, 90, true)]
    [InlineData(88, 90, true)]
    
    [InlineData(0, 10, true)]
    [InlineData(9, 10, true)]
    [InlineData(10, 10, false)]
    [InlineData(90, 10, false)]
    
    [InlineData(0, 1, true)]
    [InlineData(1, 1, false)]
    
    //if you set the CancelADateRangeMaximumDays config to zero, no values are valid!
    [InlineData(0, 0, false)]
    [InlineData(1, 0, false)]
    public void ExpectedOutcomes_WhenToDate_MoreThanIncrementDays_AfterStartDate_WithMaxConfiguration(int incrementDays, int cancelADateRangeMaximumDays, bool expectedOutcome)
    {
        _availabilityConfig.Setup(x => x.Value).Returns(new ChangeAvailabilityOptions
        {
            CancelADateRangeMaximumDays = cancelADateRangeMaximumDays
        });

        var sut = new ProposeCancelDateRangeRequestValidator(_timeProvider.Object, _availabilityConfig.Object);

        var startDate = DateOnly.Parse("2077-04-01");
        var endDate = startDate.AddDays(incrementDays);
        
        var request = new ProposeCancelDateRangeRequest(
            "test-site-123",
            startDate,
            endDate);

        var result = sut.Validate(request);
        result.IsValid.Should().Be(expectedOutcome);
    }

    [Fact]
    public void PassesValidation_WhenFromAndToAreTheSameDate()
    {
        var request = new ProposeCancelDateRangeRequest
        (
            "SiteA",
            DateOnly.Parse("2077-01-01"),
            DateOnly.Parse("2077-01-01")
        );

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
