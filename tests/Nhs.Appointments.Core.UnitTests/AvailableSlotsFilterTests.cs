using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Core.UnitTests;

public class AvailableSlotsFilterTests
{
    private readonly IAvailableSlotsFilter _sut;

    public AvailableSlotsFilterTests() => _sut = new AvailableSlotsFilter();


    [Fact]
    public void ReturnsAllSlots_WhenAttendeeListIsNull()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["COVID"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"])
        };

        var result = _sut.FilterAvailableSlots(slots, null);

        result.Count().Should().Be(3);
        result.Should().BeEquivalentTo(slots);
    }

    [Fact]
    public void ReturnsAllSlots_WhenAttendeeListIsEmpty()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["COVID"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"])
        };

        var result = _sut.FilterAvailableSlots(slots, []);

        result.Count().Should().Be(3);
        result.Should().BeEquivalentTo(slots);
    }

    [Fact]
    public void ReturnsEmptyList_WhenSlotsListIsNull()
    {
        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["COVID"]) };

        var result = _sut.FilterAvailableSlots(null, attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void ReturnsEmptyList_WhenSlotsListIsEmpty()
    {
        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["COVID"]) };

        var result = _sut.FilterAvailableSlots([], attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void ReturnsEmptyList_WhenNotSlotsMatchRequestedService()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["COVID"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["Green"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void SingleAttendee_FiltersAllSlotsWithRequestedService()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["COVID"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["FLU"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(1);
        result.Single().Services.First().Should().Be("FLU");
    }

    [Fact]
    public void SingleAttendee_IgnoresSlotsWithZeroCapacity()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 0, ["FLU"]),
            SetupSlot("09:10", "09:20", 0, ["FLU"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["FLU"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(1);
        result.First().Should().BeEquivalentTo(slots[2]);
    }

    [Fact]
    public void SingleAttendee_ReturnsSlotsSortedByTime()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:20", "09:30", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["FLU"]),
            SetupSlot("09:00", "09:10", 2, ["FLU"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["FLU"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(2);
        result.First().Should().BeEquivalentTo(slots[2]);
        result.Skip(1).First().Should().BeEquivalentTo(slots[1]);
    }

    [Fact]
    public void TwoAttendees_SameService_FindsConsecutiveSlots()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["RSV"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"]),
            SetupSlot("09:20", "09:30", 2, ["RSV"]),
            SetupSlot("09:30", "09:40", 2, ["RSV"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["RSV"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.All(r => r.Services.First() == "RSV").Should().BeTrue();
    }

    [Fact]
    public void TwoAttendees_SameService_NoConsecutiveSlots_ReturnsEmpty()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["FLU"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"]),
            SetupSlot("09:20", "09:30", 2, ["RSV"]),
            SetupSlot("09:30", "09:40", 2, ["COVID"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["RSV"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void ThreeAttendees_SameService_FindsThreeConsecutiveSlots()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["RSV"]),
            SetupSlot("09:20", "09:30", 2, ["RSV"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"]),
            SetupSlot("09:40", "09:50", 2, ["RSV"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["RSV"]), SetupAttendee(["RSV"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees).ToList();

        result.Count().Should().Be(3);
        result.All(r => r.Services.First() == "RSV").Should().BeTrue();
    }

    [Fact]
    public void MultipleAttendees_SameService_ReturnsEmptyList_WhenSlotsAreNotExactlyConsecutiveByTime()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:20", "09:30", 2, ["RSV"]),
            SetupSlot("09:40", "09:50", 2, ["RSV"]),
            SetupSlot("10:00", "10:10", 2, ["RSV"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["RSV"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void TwoAttendees_DifferentServices_FindsEitherOrderConsecutive()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["FLU"]),
            SetupSlot("09:20", "09:30", 2, ["RSV"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"]),
            SetupSlot("09:30", "09:40", 2, ["RSV"]),
            SetupSlot("09:40", "09:50", 2, ["FLU"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["FLU"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(6);
        result.Count(s => s.Services.First() == "FLU").Should().Be(3);
        result.Count(s => s.Services.First() == "RSV").Should().Be(3);
    }

    [Fact]
    public void ThreeAttendees_DifferentServices_AnyOrderConsecutiveMatch()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["FLU"]),
            SetupSlot("09:20", "09:30", 2, ["COVID"]),
            SetupSlot("09:20", "09:30", 2, ["COVID"]),
            SetupSlot("09:30", "09:40", 2, ["FLU"]),
            SetupSlot("09:40", "09:50", 2, ["RSV"])
        };
        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["FLU"]), SetupAttendee(["COVID"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(5);
        result.Count(s => s.Services.First() == "FLU").Should().Be(2);
        result.Count(s => s.Services.First() == "RSV").Should().Be(2);
        result.Count(s => s.Services.First() == "COVID").Should().Be(1);
    }

    [Fact]
    public void DifferentServices_MissingOneService_ReturnsEmpty()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["FLU"]),
            SetupSlot("09:20", "09:30", 2, ["RSV"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"]),
            SetupSlot("09:30", "09:40", 2, ["RSV"]),
            SetupSlot("09:40", "09:50", 2, ["FLU"])
        };
        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["COVID"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void ConsecutiveSlots_ExactEndToStartMatchRequired()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["RSV"]),
            SetupSlot("09:30", "09:40", 2, ["RSV"]),
            SetupSlot("09:50", "10:00", 2, ["RSV"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["RSV"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(2);
    }

    [Fact]
    public void ConsecutiveSlots_AreOrderedByFromTimeAscending()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["RSV"]),
            SetupSlot("09:20", "09:30", 2, ["RSV"]),
            SetupSlot("09:30", "09:40", 2, ["RSV"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["RSV"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.First().From.Should().Be(TestDateAt("09:00"));
        result.Last().From.Should().Be(TestDateAt("09:30"));
    }

    [Fact]
    public void ConsecutiveSlots_UnorderedInput_StillWorks()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:30", "09:40", 2, ["RSV"]),
            SetupSlot("09:00", "09:10", 2, ["RSV"]),
            SetupSlot("09:20", "09:30", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["RSV"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["RSV"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.First().From.Should().Be(TestDateAt("09:00"));
        result.Last().From.Should().Be(TestDateAt("09:30"));
    }

    [Fact]
    public void MultipleAttendees_MultipleServices_DifferentSlotLengths_ReturnCorrectConsecutiveSlots()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:05", 2, ["RSV"]),
            SetupSlot("09:05", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:15", 2, ["RSV"]),
            SetupSlot("09:15", "09:20", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["FLU"]),
            SetupSlot("09:20", "09:30", 2, ["FLU"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["FLU"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.Count(s => s.Services.First() == "RSV").Should().Be(2);
        result.Count(s => s.Services.First() == "FLU").Should().Be(2);
    }

    [Fact]
    public void MultipleAttendees_MultipleServicesInSlots_DifferentSlotLengths_ReturnCorrectConsecutiveSlots()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:05", 2, ["RSV"]),
            SetupSlot("09:05", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:15", 2, ["RSV", "COVID"]),
            SetupSlot("09:15", "09:20", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["FLU", "RSV"]),
            SetupSlot("09:20", "09:30", 2, ["FLU", "RSV"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["FLU"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.Count(s => s.Services.First() == "RSV").Should().Be(2);
        result.Count(s => s.Services.First() == "FLU").Should().Be(2);
    }

    [Fact]
    public void MuralScenario__QueryForC_and_B()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:05", 2, ["RSV", "FLU"]),
            SetupSlot("09:05", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:15", 2, ["RSV", "FLU"]),
            SetupSlot("09:15", "09:20", 2, ["RSV"]),
            SetupSlot("09:20", "09:25", 2, ["FLU"]),
            SetupSlot("09:25", "09:30", 2, ["RSV", "FLU"]),
            SetupSlot("09:30", "09:35", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["COVID"]),
            SetupSlot("09:30", "09:40", 2, ["COVID"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["FLU"]), SetupAttendee(["COVID"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.Single(slot =>
            slot.From == TestDateAt("09:10") &&
            slot.Until == TestDateAt("09:20") &&
            slot.Services.SequenceEqual(["COVID"]));
        result.Single(slot =>
            slot.From == TestDateAt("09:30") &&
            slot.Until == TestDateAt("09:40") &&
            slot.Services.SequenceEqual(["COVID"]));
        result.Single(slot =>
            slot.From == TestDateAt("09:20") &&
            slot.Until == TestDateAt("09:25") &&
            slot.Services.SequenceEqual(["FLU"]));
        result.Single(slot =>
            slot.From == TestDateAt("09:25") &&
            slot.Until == TestDateAt("09:30") &&
            slot.Services.SequenceEqual(["RSV", "FLU"]));
    }

    [Fact]
    public void MuralScenario__QueryForA_and_B()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:05", 2, ["RSV", "FLU"]),
            SetupSlot("09:05", "09:10", 2, ["RSV"]),
            SetupSlot("09:10", "09:15", 2, ["RSV", "FLU"]),
            SetupSlot("09:15", "09:20", 2, ["RSV"]),
            SetupSlot("09:20", "09:25", 2, ["FLU"]),
            SetupSlot("09:25", "09:30", 2, ["RSV", "FLU"]),
            SetupSlot("09:30", "09:35", 2, ["RSV"]),
            SetupSlot("09:10", "09:20", 2, ["COVID"]),
            SetupSlot("09:30", "09:40", 2, ["COVID"])
        };

        var attendees = new List<Attendee> { SetupAttendee(["RSV"]), SetupAttendee(["COVID"]) };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.Single(slot =>
            slot.From == TestDateAt("09:10") &&
            slot.Until == TestDateAt("09:20") &&
            slot.Services.SequenceEqual(["COVID"]));
        result.Single(slot =>
            slot.From == TestDateAt("09:30") &&
            slot.Until == TestDateAt("09:40") &&
            slot.Services.SequenceEqual(["COVID"]));
        result.Single(slot =>
            slot.From == TestDateAt("09:05") &&
            slot.Until == TestDateAt("09:10") &&
            slot.Services.SequenceEqual(["RSV"]));
        result.Single(slot =>
            slot.From == TestDateAt("09:25") &&
            slot.Until == TestDateAt("09:30") &&
            slot.Services.SequenceEqual(["RSV", "FLU"]));
    }

    [Fact]
    public void QueryForThreeServies()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:05", 3, ["RSV", "FLU"]),
            SetupSlot("09:05", "09:10", 3, ["RSV", "COVID"]),
            SetupSlot("09:10", "09:15", 3, ["RSV", "FLU"]),
            SetupSlot("09:15", "09:20", 3, ["RSV"]),
            SetupSlot("09:20", "09:25", 3, ["FLU", "COVID"]),
            SetupSlot("09:25", "09:30", 3, ["RSV", "FLU"]),
            SetupSlot("09:30", "09:35", 3, ["RSV"]),
            SetupSlot("09:30", "09:40", 3, ["COVID"])
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["COVID"]),
            SetupAttendee(["FLU"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(8);
        result.Single(slot =>
            slot.From == TestDateAt("09:00") &&
            slot.Until == TestDateAt("09:05") &&
            slot.Services.SequenceEqual(["RSV", "FLU"]));
        result.Single(slot =>
            slot.From == TestDateAt("09:05") &&
            slot.Until == TestDateAt("09:10") &&
            slot.Services.SequenceEqual(["RSV", "COVID"]));
        result.Single(slot =>
            slot.From == TestDateAt("09:10") &&
            slot.Until == TestDateAt("09:15") &&
            slot.Services.SequenceEqual(["RSV", "FLU"]));
    }

    [Fact]
    public void QueryForThreeServices_InMixedDateTimeOrder_ShouldGiveSameResult()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:10", "09:15", 3, ["RSV", "FLU"]),
            SetupSlot("09:20", "09:25", 3, ["FLU", "COVID"]),
            SetupSlot("09:15", "09:20", 3, ["RSV"]),
            SetupSlot("09:05", "09:10", 3, ["RSV", "COVID"]),
            SetupSlot("09:30", "09:35", 3, ["RSV"]),
            SetupSlot("09:30", "09:40", 3, ["COVID"]),
            SetupSlot("09:25", "09:30", 3, ["RSV", "FLU"]),
            SetupSlot("09:00", "09:05", 3, ["RSV", "FLU"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["COVID"]),
            SetupAttendee(["FLU"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(8);
        result.Single(slot =>
            slot.From == TestDateAt("09:00") &&
            slot.Until == TestDateAt("09:05") &&
            slot.Services.SequenceEqual(["RSV", "FLU"]));
        result.Single(slot =>
            slot.From == TestDateAt("09:05") &&
            slot.Until == TestDateAt("09:10") &&
            slot.Services.SequenceEqual(["RSV", "COVID"]));
        result.Single(slot =>
            slot.From == TestDateAt("09:10") &&
            slot.Until == TestDateAt("09:15") &&
            slot.Services.SequenceEqual(["RSV", "FLU"]));
    }

    [Fact]
    public void NotEnoughAvailabilityForAmountOfAttendeesRquired()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot("09:00", "09:15", 1, ["RSV"]),
            SetupSlot("09:15", "09:30", 1, ["RSV"]),
            SetupSlot("09:30", "09:45", 1, ["RSV"]),
            SetupSlot("09:45", "10:00", 1, ["RSV"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(0);
    }

    private DateTime TestDateAt(string time)
    {
        var hour = time.Split(':')[0];
        var minute = time.Split(':')[1];

        return new DateTime(2025, 10, 10, int.Parse(hour), int.Parse(minute), 0);
    }

    private SessionInstance SetupSlot(string from, string until, int capacity, string[] services)
    {
        var fromDateTime = TestDateAt(from);
        var untilDateTime = TestDateAt(until);

        var slotLength = (untilDateTime - fromDateTime).Minutes;

        return new SessionInstance(fromDateTime, untilDateTime)
        {
            Capacity = capacity, Services = services, SlotLength = slotLength
        };
    }

    private static Attendee SetupAttendee(string[] services) => new() { Services = services };
}
