using FluentAssertions;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Availability;

public class DailyAvailabilityGrouperTests
{
    private readonly DailyAvailabilityGrouper _sut = new();
    
    /*[Fact]
    public void DailyAvailabilityGrouper_ReturnsError_WhenBlocksIsNull()
    {
        var slotDuration = 5;
        var act = () => _sut.GroupAvailability(null, slotDuration);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DailyAvailabilityGrouper_ReturnsError_WhenSlotDurationIsOutOfRange()
    {
        var slotDuration = 0; 
        var blocks = AvailabilityHelper.CreateTestBlocks("09:00-10:00");
        var act = () => _sut.GroupAvailability(blocks, slotDuration);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void DailyAvailabilityGrouper_ReturnsNoAvailability_WhenSessionBlocksAreEmpty()
    {
        var slotDuration = 5;
        var blocks = Enumerable.Empty<SessionInstance>();
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(11, 59), 0),
            new (new TimeOnly(12, 0), new TimeOnly(23, 59), 0),
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void DailyAvailabilityGrouper_ReturnsAmAvailability_WhenSessionBlocksAreAmOnly()
    {
        var slotDuration = 5;
        var blocks = AvailabilityHelper.CreateTestBlocks("11:00-12:00");
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(11, 59), 12),
            new (new TimeOnly(12, 0), new TimeOnly(23, 59), 0),
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void DailyAvailabilityGrouper_ReturnsPmAvailabilityOnly_WhenSessionBlocksArePmOnly()
    {
        var slotDuration = 5;
        var blocks = AvailabilityHelper.CreateTestBlocks("12:00-13:00");
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(11, 59), 0),
            new (new TimeOnly(12, 0), new TimeOnly(23, 59), 12),
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void DailyAvailabilityGrouper_ReturnsAmPmAvailability_WhenSessionBlockSplitBetweenAmAndPm()
    {
        var slotDuration = 5;
        var blocks = AvailabilityHelper.CreateTestBlocks("11:00-13:00");
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(11, 59), 12),
            new (new TimeOnly(12, 0), new TimeOnly(23, 59), 12),
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void DailyAvailabilityGrouper_ReturnsCorrectAvailabilityCount_WhenTotalDurationIsNotDivisibleBySlotDuration()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("11:30-13:00");
        var slotDuration = 8; 
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(11, 59), 3),
            new (new TimeOnly(12, 0), new TimeOnly(23, 59), 7),
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact] public void DailyAvailabilityGrouper_ReturnsCorrectDailyAvailabilityCount_WhenThereAreMultipleSessionHolders()
    {
        var slotDuration = 5;
        var blocks = AvailabilityHelper.CreateTestBlocks("11:00-13:00|SessionHolder-One", "11:00-12:00|SessionHolder-Two"); 
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(11, 59), 24),
            new (new TimeOnly(12, 0), new TimeOnly(23, 59), 12),
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }*/
}