using FluentAssertions;
using Nhs.Appointments.Api.Availability;

namespace Nhs.Appointments.Api.Tests.Availability;

public class HourlyAvailabilityGrouperTests
{
    private readonly HourlyAvailabilityGrouper _sut = new();
    
    /*[Fact]
    public void HourlyAvailabilityGrouper_ReturnsCorrectHourlyAvailabilityCount_WhenBlockDurationIsNotDivisibleBySlotDuration()
    {
        var slotDuration = 8;
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-13:00");
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(10, 0), new TimeOnly(11, 00), 8),
            new (new TimeOnly(11, 0), new TimeOnly(12, 00), 7),
            new (new TimeOnly(12, 0), new TimeOnly(13, 00), 7)
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void HourlyAvailabilityGrouper_ReturnsCorrectHourlyAvailabilityCountForASingleHour_WhenBlockDurationIsNotDivisibleBySlotDuration()
    {
        var slotDuration = 8;
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(10, 0), new TimeOnly(11, 00), 7),
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact] public void HourlyAvailabilityGrouper_ReturnsCorrectHourlyAvailabilityCount_WhenBlockDurationIsDivisibleBySlotDuration()
    {
        var slotDuration = 5;
        var blocks = AvailabilityHelper.CreateTestBlocks("11:00-13:00");
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(11, 0), new TimeOnly(12, 00), 12),
            new (new TimeOnly(12, 0), new TimeOnly(13, 00), 12)
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact] public void HourlyAvailabilityGrouper_ReturnsCorrectHourlyAvailabilityCount_WhenThereAreMultipleSessionHolders()
    {
        var slotDuration = 5;
        var blocks = AvailabilityHelper.CreateTestBlocks("09:00-10:00|SessionHolder-One", "09:00-11:00|SessionHolder-Two"); 
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(9, 0), new TimeOnly(10, 00), 24),
            new (new TimeOnly(10, 0), new TimeOnly(11, 00), 12)
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void HourlyAvailabilityGrouper_ReturnsError_WhenBlocksIsNull()
    {
        var slotDuration = 5;
        var act = () => _sut.GroupAvailability(null, slotDuration);
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void HourlyAvailabilityGrouper_ReturnsError_WhenSlotDurationIsOutOfRange()
    {
        var slotDuration = 0;
        var blocks = AvailabilityHelper.CreateTestBlocks("09:00-10:00"); 
        var act = () => _sut.GroupAvailability(blocks, slotDuration);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }*/
}