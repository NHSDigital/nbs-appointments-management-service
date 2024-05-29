using Nhs.Appointments.Core.Extensions;

namespace Nhs.Appointments.Core.UnitTests.Extensions;

public class ScheduleTemplateExtensionsTests
{
    [Theory] 
    [InlineData(new[] { "service1", "services2" }, 1)] 
    [InlineData(new[] { "service1", "services2", "service3" }, 2)] 
    public void RemoveServices_RemovesServices_WhenASingleScheduleBlockHasMultipleServicesAndASingleServiceIsRemoved(string[] services, int expectedNumberOfServices)
    {
        var templateSchedule = CreateScheduleTemplateWithSingleBlock(services);
            
        var cleanedTemplate= templateSchedule.RemoveServices(new [] { "service1" });

        cleanedTemplate.Items.Length.Should().Be(1);
        cleanedTemplate.Items.First().ScheduleBlocks.Length.Should().Be(1);
        cleanedTemplate.Items.First().ScheduleBlocks.First().Services.Length.Should().Be(expectedNumberOfServices);        
    }
    
    [Fact] 
    public void RemoveServices_RemovesASchedule_WhenASingleScheduleBlockHasASingleServiceAndThatServiceIsRemoved()
    {
        var templateSchedule = CreateScheduleTemplateWithSingleBlock(new[] { "service1" });            
        var cleanedTemplate= templateSchedule.RemoveServices(new [] { "service1" });
        cleanedTemplate.Items.Length.Should().Be(0);
    }

    [Fact]
    public void RemoveServices_RemovesASchedule_WhenTwoScheduleBlocksOneWithMultipleServicesTheOtherWithSingleServiceAndSingleServiceIsRemoved()
    {
        var templateSchedule = CreateScheduleTemplateWithTwoBlocks(new[] { "service1" }, new[] { "service1", "service2" });

        var cleanedTemplate = templateSchedule.RemoveServices(new[] { "service1" });

        cleanedTemplate.Items.Length.Should().Be(1);
        cleanedTemplate.Items.First().ScheduleBlocks.Length.Should().Be(1);
    }

    private static WeekTemplate CreateScheduleTemplateWithTwoBlocks(string[] servicesScheduleBlock1, string[] servicesScheduleBlock2)
    {
        var scheduleBlock1 = new ScheduleBlock() { From = TimeOnly.MinValue, Until = TimeOnly.MaxValue, Services = servicesScheduleBlock1};
        var scheduleBlock2 = new ScheduleBlock() { From = TimeOnly.MinValue, Until = TimeOnly.MaxValue, Services = servicesScheduleBlock2};
        return CreateWeekTemplate(scheduleBlock1, scheduleBlock2);
    }

    private static WeekTemplate CreateScheduleTemplateWithSingleBlock(string[] services)
    {
        var scheduleBlock = new ScheduleBlock() { From = TimeOnly.MinValue, Until = TimeOnly.MaxValue, Services = services };
        return CreateWeekTemplate(scheduleBlock);
    }

    private static WeekTemplate CreateWeekTemplate(params ScheduleBlock[] scheduleBlocks)
    {
        return  new WeekTemplate()
        {
            Site = "1001",
            Items = new[]
            {
                new Schedule
                {
                    Days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday },
                    ScheduleBlocks = scheduleBlocks
                }
            }
        }; 
    }
}