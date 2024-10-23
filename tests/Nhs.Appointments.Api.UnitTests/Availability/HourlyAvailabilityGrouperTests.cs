using FluentAssertions;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Availability;

public class HourlyAvailabilityGrouperTests
{
    private static readonly DateOnly Date = new DateOnly(2077, 1, 1);
    private readonly HourlyAvailabilityGrouper _sut = new();
    
    [Fact]
    public void HourlyAvailabilityGrouper_ReturnsCorrectHourlyAvailabilityCount_SingleCapacitySlots()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(10,0), new TimeOnly(13,0), TimeSpan.FromMinutes(5));
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(10, 0), new TimeOnly(11, 00), 12),
            new (new TimeOnly(11, 0), new TimeOnly(12, 00), 12),
            new (new TimeOnly(12, 0), new TimeOnly(13, 00), 12)
        };
        var result = _sut.GroupAvailability(slots);
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void HourlyAvailabilityGrouper_ReturnsCorrectHourlyAvailabilityCount_MultiCapacitySlots()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(10, 0), new TimeOnly(13, 0), TimeSpan.FromMinutes(5), 3);
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(10, 0), new TimeOnly(11, 00), 36),
            new (new TimeOnly(11, 0), new TimeOnly(12, 00), 36),
            new (new TimeOnly(12, 0), new TimeOnly(13, 00), 36)
        };
        var result = _sut.GroupAvailability(slots);
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void HourlyAvailabilityGrouper_AmalgamatesMatchingSlots()
    {
        var slots = new[]
        {
            new SessionInstance(Date.ToDateTime(new TimeOnly(9,0)), Date.ToDateTime(new TimeOnly(9,10))){Capacity = 1},
            new SessionInstance(Date.ToDateTime(new TimeOnly(9,0)), Date.ToDateTime(new TimeOnly(9,10))){Capacity = 2},
        };

        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(9, 0), new TimeOnly(10, 00), 3),            
        };
        var result = _sut.GroupAvailability(slots);
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void HourlyAvailabilityGrouper_ReturnsError_WhenBlocksIsNull()
    {
        var act = () => _sut.GroupAvailability(null);
        act.Should().Throw<ArgumentNullException>();
    }    
}