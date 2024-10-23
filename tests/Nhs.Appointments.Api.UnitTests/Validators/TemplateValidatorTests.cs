using FluentAssertions;
using FluentValidation.TestHelper;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class TemplateValidatorTests
{
    private readonly TemplateValidator _sut = new();
    
    [Fact]
    public void Validate_ReturnsValidResult_WhenAllFieldsAreValid()
    {
        var template = new Template()
            {
                Days = 
                    [
                        DayOfWeek.Monday, 
                        DayOfWeek.Tuesday, 
                        DayOfWeek.Wednesday, 
                        DayOfWeek.Thursday, 
                        DayOfWeek.Friday, 
                        DayOfWeek.Saturday, 
                        DayOfWeek.Sunday
                    ],
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
            };
        var result = _sut.TestValidate(template);
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenDayIsDuplicated()
    {
        var template = new Template()
        {
            Days = 
            [
                DayOfWeek.Monday,
                DayOfWeek.Monday,
            ],
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
        };
        var result = _sut.TestValidate(template);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Template.Days));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenNoDaysAreSent()
    {
        var template = new Template()
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
        };
        var result = _sut.TestValidate(template);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Template.Days));
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenSessionIsInvalid()
    {
        var template = new Template()
        {
            Days = [DayOfWeek.Monday],
            Sessions =
            [
                new Session()
                {
                    Capacity = 0,
                    From = new TimeOnly(09, 00),
                    Until = new TimeOnly(10, 00),
                    SlotLength = 5,
                    Services = ["Service 1"]
                }
            ]
        };
        var result = _sut.TestValidate(template);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Validate_ReturnsError_WhenSessionIsNull()
    {
        var template = new Template()
        {
            Days = [DayOfWeek.Monday],
            Sessions = Array.Empty<Session>()
        };
        var result = _sut.TestValidate(template);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().PropertyName.Should().Contain(nameof(Template.Sessions));
    }
}
