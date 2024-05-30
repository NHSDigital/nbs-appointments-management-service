using FluentAssertions;
using Nhs.Appointments.Api.Availability;

namespace Nhs.Appointments.Api.Tests.Availability;

public class AvailabilityGrouperFactoryTests
{
    private readonly AvailabilityGrouperFactory _sut = new();
    
    [Fact]
    public void AvailabilityGroupFactory_CreatesDailyAvailabilityGrouper_WhenQueryTypeIsDays()
    {
        var queryType = QueryType.Days;
        var result = _sut.Create(queryType);
        result.Should().NotBeOfType<DailyAvailabilityGrouper>();
    }
    
    [Fact]
    public void AvailabilityGroupFactory_CreatesHourlyAvailabilityGrouper_WhenQueryTypeIsHours()
    {
        var queryType = QueryType.Hours;
        var result = _sut.Create(queryType);
        result.Should().BeOfType<HourlyAvailabilityGrouper>();
    }
    
    [Fact]
    public void AvailabilityGroupFactory_CreatesSlotAvailabilityGrouper_WhenQueryTypeIsSlots()
    {
        var queryType = QueryType.Slots;
        var result = _sut.Create(queryType);
        result.Should().BeOfType<SlotAvailabilityGrouper>();
    }
    
    [Fact]
    public void AvailabilityGroupFactory_ReturnsException_WhenQueryTypeIsNotSet()
    {
        var queryType = QueryType.NotSet;
        var act = () => _sut.Create(queryType);
        act.Should().Throw<NotSupportedException>()
            .WithMessage("NotSet is not a valid queryType");
    }
}