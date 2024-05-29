using FluentAssertions;
using Nhs.Appointments.Api.Validators;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Validators;

public class WeekTemplateValidatorTests
{
    private readonly WeekTemplateValidator _sut = new();


    [Fact]
    public void Validate_ReturnsSuccess_WhenRequestIsValid()
    {
        var request = GetWeekTemplate(
            "TemplateName",
            new DayOfWeek[] {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
                DayOfWeek.Sunday
            });
        
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().HaveCount(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ReturnsError_WhenNameIsNullOrEmpty(string name)
    {
        var request = GetWeekTemplate(name, new[] { DayOfWeek.Monday });
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }
    
    [Fact]
    public void Validate_ReturnsError_WhenADayIsSpecifiedMoreThanOnce()
    {
        var request = GetWeekTemplate("TemplateName", new[] { DayOfWeek.Monday, DayOfWeek.Monday });
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    private static WeekTemplate GetWeekTemplate(string name, DayOfWeek[] days)
    {
        var weekTemplate = new WeekTemplate()
        {
            Name = name,
            Id = "",
            Site = "1000",
            Items =
            new Schedule[] {
                new Schedule
                {
                    Days = days,
                    ScheduleBlocks =
                    new ScheduleBlock[]{
                        new ScheduleBlock
                        {
                            From = new TimeOnly(9, 0, 0),
                            Until = new TimeOnly(17, 0, 0),
                            Services = new []{"COVID" }
                        }
                    }
                }
            }
        };
        return weekTemplate;
    }
}