using FluentAssertions;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Availability;

public class SlotAvailabilityGrouperTests
{
    private static readonly DateOnly Date = new DateOnly(2077, 1, 1);
    private readonly SlotAvailabilityGrouper _sut = new();
    
    [Fact]
    public void SlotAvailabilityGrouper_ReturnsCorrectAvailabilitySlots_SingleCapacitySlots()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(9,0), new TimeOnly(9,30), TimeSpan.FromMinutes(5));
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(9, 0), new TimeOnly(9, 5), 1),
            new (new TimeOnly(9, 5), new TimeOnly(9, 10), 1),
            new (new TimeOnly(9, 10), new TimeOnly(9, 15), 1),
            new (new TimeOnly(9, 15), new TimeOnly(9, 20), 1),
            new (new TimeOnly(9, 20), new TimeOnly(9, 25), 1),
            new (new TimeOnly(9, 25), new TimeOnly(9, 30), 1),
        };
        var result = _sut.GroupAvailability(slots);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void SlotAvailabilityGrouper_ReturnsCorrectAvailabilitySlots_MultiCapacitySlots()
    {
        var blocks = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(9, 0), new TimeOnly(9, 30), TimeSpan.FromMinutes(5), 3);
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(9, 0), new TimeOnly(9, 5), 3),
            new (new TimeOnly(9, 5), new TimeOnly(9, 10), 3),
            new (new TimeOnly(9, 10), new TimeOnly(9, 15), 3),
            new (new TimeOnly(9, 15), new TimeOnly(9, 20), 3),
            new (new TimeOnly(9, 20), new TimeOnly(9, 25), 3),
            new (new TimeOnly(9, 25), new TimeOnly(9, 30), 3),
        };
        var result = _sut.GroupAvailability(blocks);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact] public void SlotAvailabilityGrouper_AmalgamatesMatchingSlots()
    {
        var slots = new[]
        {
            new SessionInstance(Date.ToDateTime(new TimeOnly(9,0)), Date.ToDateTime(new TimeOnly(9,10))){Capacity = 1},
            new SessionInstance(Date.ToDateTime(new TimeOnly(9,0)), Date.ToDateTime(new TimeOnly(9,10))){Capacity = 2},
        };

        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(9, 0), new TimeOnly(9, 10), 3)            
        };
        var result = _sut.GroupAvailability(slots);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void SlotAvailabilityGrouper_ReturnsError_WhenBlocksIsNull()
    {
        var act = () => _sut.GroupAvailability(null);
        act.Should().Throw<ArgumentNullException>();
    }
}