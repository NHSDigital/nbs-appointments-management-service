using FluentAssertions;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Availability;

public class DailyAvailabilityGrouperTests
{
    private static readonly DateOnly Date = new DateOnly(2077, 1, 1);
    private readonly DailyAvailabilityGrouper _sut = new();
    
    [Fact]
    public void DailyAvailabilityGrouper_ReturnsError_WhenBlocksIsNull()
    {
        var act = () => _sut.GroupAvailability(null);
        act.Should().Throw<ArgumentNullException>();
    }    
    
    [Fact]
    public void DailyAvailabilityGrouper_ReturnsNoAvailability_WhenSessionBlocksAreEmpty()
    {        
        var slots = Enumerable.Empty<SessionInstance>();
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(12, 0), 0),
            new (new TimeOnly(12, 0), new TimeOnly(0, 0), 0),
        };
        var result = _sut.GroupAvailability(slots);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void DailyAvailabilityGrouper_ReturnsAmAvailability_WhenSessionBlocksAreAmOnly()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(11, 0), new TimeOnly(12, 0), TimeSpan.FromMinutes(5));
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(12, 0), 12),
            new (new TimeOnly(12, 0), new TimeOnly(0, 0), 0),
        };
        var result = _sut.GroupAvailability(slots);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void DailyAvailabilityGrouper_ReturnsPmAvailabilityOnly_WhenSessionBlocksArePmOnly()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(12, 0), new TimeOnly(13, 0), TimeSpan.FromMinutes(5));
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(12, 0), 0),
            new (new TimeOnly(12, 0), new TimeOnly(0, 0), 12),
        };
        var result = _sut.GroupAvailability(slots);
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public void DailyAvailabilityGrouper_ReturnsAmPmAvailability_WhenSessionBlockSplitBetweenAmAndPm()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(11, 0), new TimeOnly(13, 0), TimeSpan.FromMinutes(5));
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(12, 0), 12),
            new (new TimeOnly(12, 0), new TimeOnly(0, 0), 12),
        };
        var result = _sut.GroupAvailability(slots);
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void DailyAvailabilityGrouper_AddUpCapacity_OfMultiCapacitySlots()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(10, 0), new TimeOnly(11, 0), TimeSpan.FromMinutes(5), 2);
        var expectedResult = new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0, 0), new TimeOnly(12, 0), 24),
            new (new TimeOnly(12, 0), new TimeOnly(0, 0), 0),
        };
        var result = _sut.GroupAvailability(slots);
        result.Should().BeEquivalentTo(expectedResult);
    }
}