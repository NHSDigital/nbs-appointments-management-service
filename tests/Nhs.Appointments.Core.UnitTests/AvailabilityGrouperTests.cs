using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Core.UnitTests;
public class AvailabilityGrouperTests
{
    [Fact]
    public void BuildDayAvailability_ThrowsNullException_WhenSlotsAreNull()
    {
        var action = () => AvailabilityGrouper.BuildDayAvailability(new DateOnly(2025, 10, 1), null);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BuildDayAvailability_ReturnsEmptyBlocks_WhenSlotsListIsEmpty()
    {
        var result = AvailabilityGrouper.BuildDayAvailability(new DateOnly(2025, 10, 1), []);
        result.Blocks.Should().BeEmpty();
    }

    [Fact]
    public void BuildDayAvailability_ReturnsSingleAmBlockOnly()
    {
        var date = new DateOnly(2025, 10, 1);
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(10, 0), new TimeOnly(10, 10), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildDayAvailability(date, slots);

        result.Blocks.Count.Should().Be(1);
        result.Blocks.First().From.Should().Be("10:00");
        result.Blocks.First().Until.Should().Be("12:00");
    }

    [Fact]
    public void BuildDayAvailability_ReturnsCorrectBlockTimingForMultipleAmSlots()
    {
        var date = new DateOnly(2025, 10, 1);
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(10, 30), new TimeOnly(11, 50), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildDayAvailability(date, slots);

        result.Blocks.Count.Should().Be(1);
        result.Blocks.First().From.Should().Be("10:30");
        result.Blocks.First().Until.Should().Be("12:00");
    }

    [Fact]
    public void BuildDayAvailability_ReturnsSinglePmBlockOnly()
    {
        var date = new DateOnly(2025, 10, 1);
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(13, 0), new TimeOnly(13, 10), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildDayAvailability(date, slots);

        result.Blocks.Count.Should().Be(1);
        result.Blocks.First().From.Should().Be("12:00");
        result.Blocks.First().Until.Should().Be("13:10");
    }

    [Fact]
    public void BuildDayAvailability_ReturnsCorrectBlockTimingForMultiplePmSlots()
    {
        var date = new DateOnly(2025, 10, 1);
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(13, 0), new TimeOnly(15, 30), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildDayAvailability(date, slots);

        result.Blocks.Count.Should().Be(1);
        result.Blocks.First().From.Should().Be("12:00");
        result.Blocks.First().Until.Should().Be("15:30");
    }

    [Fact]
    public void BuildDayAvailability_ReturnsAmAndPmBlock_ForSingleSpillOverSlot()
    {
        var date = new DateOnly(2025, 10, 1);
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(11, 50), new TimeOnly(12, 10), TimeSpan.FromMinutes(20));

        var result = AvailabilityGrouper.BuildDayAvailability(date, slots);

        result.Blocks.Count.Should().Be(2);
        result.Blocks.First().From.Should().Be("11:50");
        result.Blocks.First().Until.Should().Be("12:00");
        result.Blocks.Last().From.Should().Be("12:00");
        result.Blocks.Last().Until.Should().Be("12:10");
    }

    [Fact]
    public void BuildDayAvailability_ReturnAmAndPmBlock_WithCorrectTimes_ForMultipleSlots()
    {
        var date = new DateOnly(2025, 10, 1);
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(10, 50), new TimeOnly(13, 30), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildDayAvailability(date, slots);

        result.Blocks.Count.Should().Be(2);
        result.Blocks.First().From.Should().Be("10:50");
        result.Blocks.First().Until.Should().Be("12:00");
        result.Blocks.Last().From.Should().Be("12:00");
        result.Blocks.Last().Until.Should().Be("13:30");
    }

    [Fact]
    public void BuildDayAvailability_ReturnsSingleAmBlock_WhenSlotsEndAtNoon()
    {
        var date = new DateOnly(2025, 10, 1);
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(10, 50), new TimeOnly(11, 50), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildDayAvailability(date, slots);

        result.Blocks.Count.Should().Be(1);
        result.Blocks.First().From.Should().Be("10:50");
        result.Blocks.First().Until.Should().Be("12:00");
    }

    [Fact]
    public void BuildDayAvailability_ReturnsSinglePmBlock_WhenSlotsStartAtNoon()
    {
        var date = new DateOnly(2025, 10, 1);
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(12, 00), new TimeOnly(13, 00), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildDayAvailability(date, slots);

        result.Blocks.Count.Should().Be(1);
        result.Blocks.First().From.Should().Be("12:00");
        result.Blocks.First().Until.Should().Be("13:00");
    }

    [Fact]
    public void BuildDayAvailability_EnsureCorrectTimeFormat_ForAmAndPmBlocks()
    {
        var date = new DateOnly(2025, 10, 1);
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(11, 30, 10), new TimeOnly(13, 30, 10), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildDayAvailability(date, slots);

        result.Blocks.Count.Should().Be(2);
        result.Blocks.First().From.Should().Be("11:30");
        result.Blocks.Last().Until.Should().Be("13:30");
    }

    [Fact]
    public void BuildDayAvailability_EnsuresLeadZeroFormatOnMinutes()
    {
        var date = new DateOnly(2025, 10, 1);
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(11, 5), new TimeOnly(13, 5), TimeSpan.FromMinutes(5));

        var result = AvailabilityGrouper.BuildDayAvailability(date, slots);

        result.Blocks.Count.Should().Be(2);
        result.Blocks.First().From.Should().Be("11:05");
        result.Blocks.Last().Until.Should().Be("13:05");
    }

    [Fact]
    public void BuildDayAvailability_EnsureAmAndPmSlots_WhenSlotsPassedInReverseOrder()
    {
        var date = new DateOnly(2025, 10, 1);
        var pmSlots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(13, 0), new TimeOnly(15, 0), TimeSpan.FromMinutes(10));
        var amSlots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(10, 0), new TimeOnly(11, 30), TimeSpan.FromMinutes(10));

        var combinedSlots = new List<SessionInstance>();
        combinedSlots.AddRange(pmSlots);
        combinedSlots.AddRange(amSlots);

        var result = AvailabilityGrouper.BuildDayAvailability(date, combinedSlots);

        result.Blocks.Count.Should().Be(2);
        result.Blocks.First().From.Should().Be("10:00");
        result.Blocks.Last().Until.Should().Be("15:00");
    }

    [Fact]
    public void BuildDayAvailability_EnsuresAmAndPmSlots_WhenSlotsHaveGaps()
    {
        var date = new DateOnly(2025, 10, 1);
        var amSlots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(6, 0), new TimeOnly(10, 30), TimeSpan.FromMinutes(10));
        var pmSlots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(14, 0), new TimeOnly(15, 0), TimeSpan.FromMinutes(10));

        var combinedSlots = new List<SessionInstance>();
        combinedSlots.AddRange(pmSlots);
        combinedSlots.AddRange(amSlots);

        var result = AvailabilityGrouper.BuildDayAvailability(date, combinedSlots);

        result.Blocks.Count.Should().Be(2);
        result.Blocks.First().From.Should().Be("06:00");
        result.Blocks.Last().Until.Should().Be("15:00");
    }
    
    [Fact]
    public void BuildDayAvailability_Spillover_Until_Time_Is_Greater_Than_Existing_Pm_Slots_Until_Time()
    {
        var date = new DateOnly(2025, 10, 1);
        var amSlots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(11, 55), new TimeOnly(12, 10), TimeSpan.FromMinutes(15));
        var pmSlots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(12, 00), new TimeOnly(12, 05), TimeSpan.FromMinutes(5));

        var combinedSlots = new List<SessionInstance>();
        combinedSlots.AddRange(pmSlots);
        combinedSlots.AddRange(amSlots);

        var result = AvailabilityGrouper.BuildDayAvailability(date, combinedSlots);

        result.Blocks.Count.Should().Be(2);
        result.Blocks.First().From.Should().Be("11:55");
        result.Blocks.First().Until.Should().Be("12:00");
        
        result.Blocks.Last().From.Should().Be("12:00");
        result.Blocks.Last().Until.Should().Be("12:10");
    }
}
