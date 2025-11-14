using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Core.UnitTests;
public class AvailableSlotsFilterTests
{
    private readonly IAvailableSlotsFilter _sut;

    public AvailableSlotsFilterTests()
    {
        _sut = new AvailableSlotsFilter();
    }
    
    [Fact]
    public void ReturnsAllSlots_WhenAttendeeListIsNull()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["COVID"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
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
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["COVID"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
        };

        var result = _sut.FilterAvailableSlots(slots, []);

        result.Count().Should().Be(3);
        result.Should().BeEquivalentTo(slots);
    }

    [Fact]
    public void ReturnsEmptyList_WhenSlotsListIsNull()
    {
        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["COVID"])
        };

        var result = _sut.FilterAvailableSlots(null, attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void ReturnsEmptyList_WhenSlotsListIsEmpty()
    {
        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["COVID"])
        };

        var result = _sut.FilterAvailableSlots([], attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void ReturnsEmptyList_WhenNotSlotsMatchRequestedService()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["COVID"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["Green"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void SingleAttendee_FiltersAllSlotsWithRequestedService()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["FLU"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(1);
        result.Single().Services.First().Should().Be("FLU");
    }

    [Fact]
    public void SingleAttendee_IgnoresSlotsWithZeroCapacity()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 0, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 0, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["FLU"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(1);
        result.First().Should().BeEquivalentTo(slots[2]);
    }

    [Fact]
    public void SingleAttendee_ReturnsSlotsSortedByTime()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["FLU"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["FLU"])
        };

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
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 30, 0), new DateTime(2025, 10, 10, 9, 40, 0), 2, 10, ["RSV"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.All(r => r.Services.First() == "RSV").Should().BeTrue();
    }

    [Fact]
    public void TwoAttendees_SameService_NoConsecutiveSlots_ReturnsEmpty()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 30, 0), new DateTime(2025, 10, 10, 9, 40, 0), 2, 10, ["COVID"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void ThreeAttendees_SameService_FindsThreeConsecutiveSlots()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 40, 0), new DateTime(2025, 10, 10, 9, 50, 0), 2, 10, ["RSV"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(3);
        result.All(r => r.Services.First() == "RSV").Should().BeTrue();
    }

    [Fact]
    public void MultipleAttendees_SameService_ReturnsEmptyList_WhenSlotsAreNotExactlyConsecutiveByTime()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 40, 0), new DateTime(2025, 10, 10, 9, 50, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 10, 00, 0), new DateTime(2025, 10, 10, 10, 10, 0), 2, 10, ["RSV"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void TwoAttendees_DifferentServices_FindsEitherOrderConsecutive()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 30, 0), new DateTime(2025, 10, 10, 9, 40, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 40, 0), new DateTime(2025, 10, 10, 9, 50, 0), 2, 10, ["FLU"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["FLU"])
        };

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
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["COVID"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["COVID"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 30, 0), new DateTime(2025, 10, 10, 9, 40, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 40, 0), new DateTime(2025, 10, 10, 9, 50, 0), 2, 10, ["RSV"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["FLU"]),
            SetupAttendee(["COVID"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(6);
        result.Count(s => s.Services.First() == "FLU").Should().Be(2);
        result.Count(s => s.Services.First() == "RSV").Should().Be(2);
        result.Count(s => s.Services.First() == "COVID").Should().Be(2);
    }

    [Fact]
    public void DifferentServices_MissingOneService_ReturnsEmpty()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 30, 0), new DateTime(2025, 10, 10, 9, 40, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 40, 0), new DateTime(2025, 10, 10, 9, 50, 0), 2, 10, ["FLU"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["COVID"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(0);
    }

    [Fact]
    public void ConsecutiveSlots_ExactEndToStartMatchRequired()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 30, 0), new DateTime(2025, 10, 10, 9, 40, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 50, 0), new DateTime(2025, 10, 10, 10, 00, 0), 2, 10, ["RSV"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(2);
    }

    [Fact]
    public void ConsecutiveSlots_AreOrderedByFromTimeAscending()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 30, 0), new DateTime(2025, 10, 10, 9, 40, 0), 2, 10, ["RSV"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.First().From.Should().Be(new DateTime(2025, 10, 10, 9, 0, 0));
        result.Last().From.Should().Be(new DateTime(2025, 10, 10, 9, 30, 0));
    }

    [Fact]
    public void ConsecutiveSlots_UnorderedInput_StillWorks()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 30, 0), new DateTime(2025, 10, 10, 9, 40, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["RSV"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["RSV"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.First().From.Should().Be(new DateTime(2025, 10, 10, 9, 0, 0));
        result.Last().From.Should().Be(new DateTime(2025, 10, 10, 9, 30, 0));
    }

    [Fact]
    public void MultipleAttendees_MultipleServices_DifferentSlotLengths_ReturnCorrectConsecutiveSlots()
    {
        var slots = new List<SessionInstance>
        {
            SetupSlot(new DateTime(2025, 10, 10, 9, 0, 0), new DateTime(2025, 10, 10, 9, 5, 0), 2, 5, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 5, 0), new DateTime(2025, 10, 10, 9, 10, 0), 2, 5, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 15, 0), 2, 5, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 15, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 5, ["RSV"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 10, 0), new DateTime(2025, 10, 10, 9, 20, 0), 2, 10, ["FLU"]),
            SetupSlot(new DateTime(2025, 10, 10, 9, 20, 0), new DateTime(2025, 10, 10, 9, 30, 0), 2, 10, ["FLU"]),
        };

        var attendees = new List<Attendee>
        {
            SetupAttendee(["RSV"]),
            SetupAttendee(["FLU"])
        };

        var result = _sut.FilterAvailableSlots(slots, attendees);

        result.Count().Should().Be(4);
        result.Count(s => s.Services.First() == "RSV").Should().Be(2);
        result.Count(s => s.Services.First() == "FLU").Should().Be(2);
    }

    // More tests for different slot lengths
    // Scenario on the Mural board for 3 services and different slot lengths need adding as well

    private static SessionInstance SetupSlot(DateTime from, DateTime until, int capacity, int slotLength, string[] services)
    {
        return new SessionInstance(from, until)
        {
            Capacity = capacity,
            Services = services,
            SlotLength = slotLength,
        };
    }

    private static Attendee SetupAttendee(string[] services)
    {
        return new Attendee
        {
            Services = services
        };
    }
}
