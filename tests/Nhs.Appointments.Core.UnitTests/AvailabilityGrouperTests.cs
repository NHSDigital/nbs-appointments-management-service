using Nhs.Appointments.Core.Availability;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        result.Blocks.Count.Should().Be(1);
        result.Blocks.First().From.Should().Be("11:50");
        result.Blocks.First().Until.Should().Be("12:00");
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
        result.Blocks.Last().Until.Should().Be("12:05");
    }

    [Fact]
    public void BuildHourAvailability_ThrowsArgumentNullException()
    {
        var action = () => AvailabilityGrouper.BuildHourAvailability("Test Site", new DateOnly(2025, 10, 1), [], null);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BuildHourAvailability_ReturnsEmptyHoursList_WhenSlotsIsEmpty()
    {
        var date = new DateOnly(2025, 10, 1);
        var attendees = new List<Attendee>
        {
            new() { Services = ["RSV:Adult"] },
            new() { Services = ["RSV:Adult"] }
        };

        var result = AvailabilityGrouper.BuildHourAvailability("Test Site", date, attendees, []);

        result.Hours.Count.Should().Be(0);
        result.Date.Should().Be(date);
        result.Site.Should().Be("Test Site");
    }

    [Fact]
    public void BuildHourAvailability_EnsureNonHourDetailsAreCorrect()
    {
        var date = new DateOnly(2025, 10, 1);
        var attendees = new List<Attendee>
        {
            new() { Services = ["RSV:Adult"] },
            new() { Services = ["RSV:Adult"] }
        };
        var site = "Test Site";
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(9, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildHourAvailability(site, date, attendees, slots);

        result.Date.Should().Be(date);
        result.Site.Should().Be(site);
        result.Attendees.Should().BeEquivalentTo(attendees);
    }

    [Fact]
    public void BuildHourAvailability_EnsureHoursOrderedByAscending()
    {
        var date = new DateOnly(2025, 10, 1);
        var attendees = new List<Attendee>
        {
            new() { Services = ["RSV:Adult"] },
            new() { Services = ["RSV:Adult"] }
        };
        var site = "Test Site";
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(9, 0), new TimeOnly(13, 0), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildHourAvailability(site, date, attendees, slots);

        result.Hours.Count.Should().Be(4);
        result.Hours.First().From.Should().Be("09:00");
        result.Hours.Skip(1).First().From.Should().Be("10:00");
        result.Hours.Skip(2).First().From.Should().Be("11:00");
        result.Hours.Skip(3).First().From.Should().Be("12:00");
    }

    [Fact]
    public void BuildHourAvailability_CorrectlyFormatsFromAndUntil()
    {
        var date = new DateOnly(2025, 10, 1);
        var attendees = new List<Attendee>
        {
            new() { Services = ["RSV:Adult"] },
            new() { Services = ["RSV:Adult"] }
        };
        var site = "Test Site";
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(9, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildHourAvailability(site, date, attendees, slots);

        result.Hours.Count.Should().Be(1);
        result.Hours.First().From.Should().Be("09:00");
        result.Hours.First().Until.Should().Be("10:00");
    }

    [Fact]
    public void BuildHourAvailability_CorrectlyFormatsMidnight()
    {
        var date = new DateOnly(2025, 10, 1);
        var attendees = new List<Attendee>
        {
            new() { Services = ["RSV:Adult"] },
            new() { Services = ["RSV:Adult"] }
        };
        var site = "Test Site";
        var slots = new List<SessionInstance>
        {
            new(date.ToDateTime(new TimeOnly(23, 0)), date.AddDays(1).ToDateTime(new TimeOnly(1, 1)))
            {
                Capacity = 1,
                Services = ["RSV:Adult"],
                SlotLength = 10
            }
        };

        var result = AvailabilityGrouper.BuildHourAvailability(site, date, attendees, slots);

        result.Hours.Count.Should().Be(1);
        result.Hours.First().From.Should().Be("23:00");
        result.Hours.First().Until.Should().Be("00:00");
    }

    [Fact]
    public void BuildHourAvailability_DoesntIncludeAllHoursInRange()
    {
        var date = new DateOnly(2025, 10, 1);
        var attendees = new List<Attendee>
        {
            new() { Services = ["RSV:Adult"] },
            new() { Services = ["RSV:Adult"] }
        };
        var site = "Test Site";
        var amSlots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(8, 0), new TimeOnly(10, 30), TimeSpan.FromMinutes(10));
        var pmSlots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(14, 0), new TimeOnly(15, 0), TimeSpan.FromMinutes(10));

        var combinedSlots = new List<SessionInstance>();
        combinedSlots.AddRange(pmSlots);
        combinedSlots.AddRange(amSlots);

        var result = AvailabilityGrouper.BuildHourAvailability(site, date, attendees, combinedSlots);

        result.Hours.Count.Should().Be(4);
        result.Hours.Any(h => h.From is "11:00" or "12:00").Should().BeFalse();
    }

    [Fact]
    public void BuildHourAvailability_OnlyProducesOneHourInList_WhenMultipleSlotsStartWithinTheHour()
    {
        var date = new DateOnly(2025, 10, 1);
        var attendees = new List<Attendee>
        {
            new() { Services = ["RSV:Adult"] },
            new() { Services = ["RSV:Adult"] }
        };
        var site = "Test Site";
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(9, 0), new TimeOnly(12, 0), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildHourAvailability(site, date, attendees, slots);

        result.Hours.Count.Should().Be(3);
        result.Hours.Count(h => h.From is "09:00").Should().Be(1);
        result.Hours.Count(h => h.Until is "10:00").Should().Be(1);
    }

    [Fact]
    public void BuildHourAvailability_OnlyProducesOneHour_WhenSlotEndsInNextHour()
    {
        var date = new DateOnly(2025, 10, 1);
        var attendees = new List<Attendee>
        {
            new() { Services = ["RSV:Adult"] },
            new() { Services = ["RSV:Adult"] }
        };
        var site = "Test Site";
        var slots = AvailabilityHelper.CreateTestSlots(date, new TimeOnly(11, 55), new TimeOnly(12, 10), TimeSpan.FromMinutes(20));

        var result = AvailabilityGrouper.BuildHourAvailability(site, date, attendees, slots);

        result.Hours.Count.Should().Be(1);
        result.Hours.First().From.Should().Be("11:00");
        result.Hours.First().Until.Should().Be("12:00");
    }

    [Fact]
    public void BuildSlotAvailability_ThrowsArgumentNullException()
    {
        var action = () => AvailabilityGrouper.BuildSlotsAvailability(
            "Test Site",
            DateTime.Parse("2025-09-01T09:00:00.000Z"), 
            DateTime.Parse("2025-09-01T16:00:00.000Z"),
            [],
            null);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BuildSlotAvailability_ReturnsEmptySlotsList_WhenSlotsIsEmpty()
    {
        var from = DateTime.Parse("2025-09-01T09:00:00.000Z");
        var until = DateTime.Parse("2025-09-01T16:00:00.000Z");

        var result = AvailabilityGrouper.BuildSlotsAvailability(
            "Test Site",
            from,
            until,
            [
                new() { Services = ["RSV:Adult"] }
            ],
            []);

        result.Slots.Count.Should().Be(0);
    }

    [Fact]
    public void BuildSlotAvailability_EnsuresNonSlotDetailsAreCorrect()
    {
        var from = DateTime.Parse("2025-09-01T09:00:00.000Z");
        var until = DateTime.Parse("2025-09-01T16:00:00.000Z");
        var attendees = new List<Attendee> { new() { Services = ["RSV:Adult"] } };

        var slots = AvailabilityHelper.CreateTestSlots(DateOnly.FromDateTime(from), new TimeOnly(9, 0), new TimeOnly(17, 0), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildSlotsAvailability(
            "Test Site",
            from,
            until,
            attendees,
            slots);

        result.Site.Should().Be("Test Site");
        result.Attendees.Should().BeEquivalentTo(attendees);
        result.From.Should().Be(from);
        result.Until.Should().Be(until);
    }

    [Fact]
    public void BuildSlotAvailability_EnsureSlotsOrderedByTimeAscending()
    {
        var from = DateTime.Parse("2025-09-01T09:00:00.000Z");
        var until = DateTime.Parse("2025-09-01T16:00:00.000Z");
        var attendees = new List<Attendee> { new() { Services = ["RSV:Adult"] } };

        var slots = AvailabilityHelper.CreateTestSlots(DateOnly.FromDateTime(from), new TimeOnly(9, 0), new TimeOnly(17, 0), TimeSpan.FromMinutes(10));

        var result = AvailabilityGrouper.BuildSlotsAvailability(
            "Test Site",
            from,
            until,
            attendees,
            slots);

        result.Slots.First().From.Should().Be("09:00");
        result.Slots.First().Until.Should().Be("09:10");
        result.Slots.Last().From.Should().Be("16:50");
        result.Slots.Last().Until.Should().Be("17:00");
    }

    [Fact]
    public void BuildSlotAvailability_CorrectlyFormatsMidnight()
    {
        var from = new DateTime(2025, 9, 1, 23, 50, 0);
        var until = DateTime.Parse("2025-09-01T16:00:00.000Z");
        var attendees = new List<Attendee> { new() { Services = ["RSV:Adult"] } };
        var slots = new List<SessionInstance>
        {
            new(from, from.AddMinutes(10))
            {
                Capacity = 1,
                Services = ["RSV:Adult"],
                SlotLength = 10
            }
        };

        var result = AvailabilityGrouper.BuildSlotsAvailability(
            "Test Site",
            from,
            until,
            attendees,
            slots);

        result.Slots.Last().From.Should().Be("23:50");
        result.Slots.Last().Until.Should().Be("00:00");
    }

    [Fact]
    public void BuildSlotAvailability_AddsCorrectServicesToCorrectSlots()
    {
        var from = new DateTime(2025, 9, 1, 10, 00, 0);
        var until = DateTime.Parse("2025-09-01T16:00:00.000Z");
        // TODO: Passing an array like this breaks the grouper
        var attendees = new List<Attendee> { new() { Services = ["RSV:Adult", "COVID:18+"] } };
        var slots = new List<SessionInstance>
        {
            new(from, from.AddMinutes(10))
            {
                Capacity = 1,
                Services = ["RSV:Adult"],
                SlotLength = 10
            },
            new(from, from.AddMinutes(10))
            {
                Capacity = 1,
                Services = ["COVID:18+"],
                SlotLength = 10
            }
        };

        var result = AvailabilityGrouper.BuildSlotsAvailability(
            "Test Site",
            from,
            until,
            attendees,
            slots);

        result.Slots.Count.Should().Be(2);
        result.Slots.First().Services.Should().BeEquivalentTo(slots.First().Services);
        result.Slots.Last().Services.Should().BeEquivalentTo(slots.Last().Services);
    }

    [Fact]
    public void BuildSlotAvailability_AddSlotsOfDifferentLengths()
    {

    }
}
