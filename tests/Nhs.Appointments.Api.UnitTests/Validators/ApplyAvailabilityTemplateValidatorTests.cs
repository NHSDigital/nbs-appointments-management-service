using Azure;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class ApplyAvailabilityTemplateValidatorTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly ApplyAvailabilityTemplateRequestValidator _sut;
    public ApplyAvailabilityTemplateValidatorTests()
    {
        _timeProvider
            .Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(DateTime.Parse("2076-12-31T00:00:00Z")));

        _sut = new ApplyAvailabilityTemplateRequestValidator(_timeProvider.Object);
    }

    [Fact]
    public void Validate_ReturnsValidResult_WhenAllFieldsAreValid()
    {
        var request = new ApplyAvailabilityTemplateRequest(
            Site: "ABC01",
            From: "2077-01-01",
            Until: "2077-01-01",
            Template: new Template()
            {
                Days = [DayOfWeek.Monday],
                Sessions =
                [
                    new Session()
                    {
                        Capacity = 1,
                        From = new TimeOnly(09, 00),
                        Until = new TimeOnly(10, 00),
                        SlotLength = 5,
                        Services = ["Service 1"]
                    }
                ]
            }
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenSiteIsInvalid(string? siteId)
    {
        var request = new ApplyAvailabilityTemplateRequest(
            Site: siteId,
            From: "2077-01-01",
            Until: "2077-01-01",
            Template: new Template()
                {
                    Days = [DayOfWeek.Monday],
                    Sessions =
                    [
                        new Session()
                        {
                            Capacity = 1,
                            From = new TimeOnly(09, 00),
                            Until = new TimeOnly(10, 00),
                            SlotLength = 5,
                            Services = ["Service 1"]
                        }
                    ]
                }
            );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(ApplyAvailabilityTemplateRequest.Site));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("2077-02-01")]
    [InlineData("01-01-2077")]
    [InlineData("2077/01/01")]
    [InlineData("2077-99-31")]
    [InlineData("2077-01-99")]
    [InlineData("Not a date")]
    public void Validate_ReturnsError_WhenFromDateIsInvalid(string? fromDate)
    {
        var request = new ApplyAvailabilityTemplateRequest(
            Site: "ABC01",
            From: fromDate,
            Until: "2077-01-01",
            Template: new Template()
            {
                Days = [DayOfWeek.Monday],
                Sessions =
                [
                    new Session()
                    {
                        Capacity = 1,
                        From = new TimeOnly(09, 00),
                        Until = new TimeOnly(10, 00),
                        SlotLength = 5,
                        Services = ["Service 1"]
                    }
                ]
            }
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(ApplyAvailabilityTemplateRequest.From));
    }

    [Fact]
    public void Validate_ReturnsError_WhenFromDateIsTodayOrEarlier()
    {
        var request = new ApplyAvailabilityTemplateRequest(
            Site: "ABC01",
            From: "2076-12-31",
            Until: "2077-02-01",
            Template: new Template()
            {
                Days = [DayOfWeek.Monday],
                Sessions =
                [
                    new Session()
                    {
                        Capacity = 1,
                        From = new TimeOnly(09, 00),
                        Until = new TimeOnly(10, 00),
                        SlotLength = 5,
                        Services = ["Service 1"]
                    }
                ]
            }
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(ApplyAvailabilityTemplateRequest.From));
        result.Errors.Single().ErrorMessage.Should().Be("'from' date must be at least 1 day in the future");
    }

    [Fact]
    public void Validate_ReturnsError_WhenUntilDateIsMoreThanOneYearInTheFuture()
    {
        var request = new ApplyAvailabilityTemplateRequest(
            Site: "ABC01",
            From: "2077-01-02",
            Until: "2078-01-03",
            Template: new Template()
            {
                Days = [DayOfWeek.Monday],
                Sessions =
                [
                    new Session()
                    {
                        Capacity = 1,
                        From = new TimeOnly(09, 00),
                        Until = new TimeOnly(10, 00),
                        SlotLength = 5,
                        Services = ["Service 1"]
                    }
                ]
            }
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(ApplyAvailabilityTemplateRequest.Until));
        result.Errors.Single().ErrorMessage.Should().Be("'until' date cannot be later than 1 year from now");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("2077/01/01")]
    [InlineData("01-01-2077")]
    [InlineData("2077-99-31")]
    [InlineData("2077-01-99")]
    [InlineData("Not a date")]
    public void Validate_ReturnsError_WhenUntilDateIsInvalid(string? untilDate)
    {
        var request = new ApplyAvailabilityTemplateRequest(
            Site: "ABC01",
            From: "2077-01-01",
            Until: untilDate,
            Template: new Template()
            {
                Days = [DayOfWeek.Monday],
                Sessions =
                [
                    new Session()
                    {
                        Capacity = 1,
                        From = new TimeOnly(09, 00),
                        Until = new TimeOnly(10, 00),
                        SlotLength = 5,
                        Services = ["Service 1"]
                    }
                ]
            }
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(ApplyAvailabilityTemplateRequest.Until));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenTemplateIsInvalid()
    {
        var request = new ApplyAvailabilityTemplateRequest(
            Site: "ABC01",
            From: "2077-01-01",
            Until: "2077-01-01",
            Template: new Template()
            {
                Days = [],
                Sessions =
                [
                    new Session()
                    {
                        Capacity = 1,
                        From = new TimeOnly(09, 00),
                        Until = new TimeOnly(10, 00),
                        SlotLength = 5,
                        Services = ["Service 1"]
                    }
                ]
            }
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenTemplateIsNull()
    {
        var request = new ApplyAvailabilityTemplateRequest(
            Site: "ABC01",
            From: "2077-01-01",
            Until: "2077-01-01",
            Template: null
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(ApplyAvailabilityTemplateRequest.Template));
    }
}
