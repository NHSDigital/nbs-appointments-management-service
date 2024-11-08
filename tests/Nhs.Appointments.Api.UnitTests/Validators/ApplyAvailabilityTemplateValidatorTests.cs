using Azure;
using FluentAssertions;
using FluentValidation.TestHelper;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class ApplyAvailabilityTemplateValidatorTests
{
    private readonly ApplyAvailabilityTemplateRequestValidator _sut = new();
    
    [Fact]
    public void Validate_ReturnsValidResult_WhenAllFieldsAreValid()
    {
        var request = new ApplyAvailabilityTemplateRequest(
            Site: "ABC01",
            From: new DateOnly(2077, 01, 01),
            Until: new DateOnly(2077, 01, 01),
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
            From: new DateOnly(2077, 01, 01),
            Until: new DateOnly(2077, 01, 01),
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

    [Fact]
    public void Validate_ReturnsError_WhenTemplateIsInvalid()
    {
        var request = new ApplyAvailabilityTemplateRequest(
            Site: "ABC01",
            From: new DateOnly(2077, 01, 01),
            Until: new DateOnly(2077, 01, 01),
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
            From: new DateOnly(2077, 01, 01),
            Until: new DateOnly(2077, 01, 01),
            Template: null
        );
        var result = _sut.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(ApplyAvailabilityTemplateRequest.Template));
    }
}
