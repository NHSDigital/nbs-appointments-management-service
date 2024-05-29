using FluentAssertions;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Availability;

public class SlotAvailabilityGrouperTests
{
    private readonly SlotAvailabilityGrouper _sut = new();
    
    [Fact]
    public void SlotAvailabilityGrouper_ReturnsCorrectAvailabilitySlots_WhenBlockDurationIsNotDivisibleBySlotDuration()
    {
        var slotDuration = 8;
        var blocks = AvailabilityHelper.CreateTestBlocks("09:00-09:30");
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(9, 0), new TimeOnly(9, 8), 1),
            new (new TimeOnly(9, 8), new TimeOnly(9, 16), 1),
            new (new TimeOnly(9, 16), new TimeOnly(9, 24), 1),
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void SlotAvailabilityGrouper_ReturnsCorrectAvailabilitySlots_WhenBlockDurationIsDivisibleBySlotDuration()
    {
        var slotDuration = 5;
        var blocks = AvailabilityHelper.CreateTestBlocks("09:00-09:30");
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(9, 0), new TimeOnly(9, 5), 1),
            new (new TimeOnly(9, 5), new TimeOnly(9, 10), 1),
            new (new TimeOnly(9, 10), new TimeOnly(9, 15), 1),
            new (new TimeOnly(9, 15), new TimeOnly(9, 20), 1),
            new (new TimeOnly(9, 20), new TimeOnly(9, 25), 1),
            new (new TimeOnly(9, 25), new TimeOnly(9, 30), 1),
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact] public void SlotAvailabilityGrouper_ReturnsCorrectSlotAvailabilityCount_WhenThereAreMultipleSessionHolders()
    {
        var slotDuration = 5;
        var blocks = AvailabilityHelper.CreateTestBlocks("09:00-09:10|SessionHolder-One", "09:00-09:20|SessionHolder-Two");
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(9, 0), new TimeOnly(9, 5), 2),
            new (new TimeOnly(9, 5), new TimeOnly(9, 10), 2),
            new (new TimeOnly(9, 10), new TimeOnly(9, 15), 1),
            new (new TimeOnly(9, 15), new TimeOnly(9, 20), 1),
        };
        var result = _sut.GroupAvailability(blocks, slotDuration);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void SlotAvailabilityGrouper_ReturnsError_WhenBlocksIsNull()
    {
        var slotDuration = 5;
        var act = () => _sut.GroupAvailability(null, slotDuration);
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void SlotAvailabilityGrouper_ReturnsError_WhenSlotDurationIsOutOfRange()
    {
        var slotDuration = 0;
        var blocks = AvailabilityHelper.CreateTestBlocks("09:00-09:30");
        var act = () => _sut.GroupAvailability(blocks, slotDuration);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}