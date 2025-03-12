namespace Nhs.Appointments.Core.UnitTests.AvailabilityCalculations;

public class SingleServiceTests : AvailabilityCalculationsBase
{
    [Fact]
    public async Task MakesNoChangesIfAllAppointmentsAreStillValid()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", "09:10", avStatus: "Supported"),
            TestBooking("2", "Green", "09:20", avStatus: "Supported"),
            TestBooking("3", "Green", "09:30", avStatus: "Supported")
        };

        var sessions = new List<SessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));

        resultingAvailabilityState.Recalculations.Should().BeEmpty();
        resultingAvailabilityState.AvailableSlots.Should().HaveCount(15);
    }

    [Fact]
    public async Task SchedulesOrphanedAppointmentsIfPossible()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", "09:10"),
            TestBooking("2", "Green", "09:20"),
            TestBooking("3", "Green", "09:30")
        };

        var sessions = new List<SessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));

        resultingAvailabilityState.Recalculations.Should().HaveCount(3);
        resultingAvailabilityState.AvailableSlots.Should().HaveCount(15);
    }

    [Fact]
    public async Task OrphansLiveAppointmentsIfTheyCannotBeFulfilled()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", "09:10", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Green", "09:10", avStatus: "Supported", creationOrder: 2)
        };

        var sessions = new List<SessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));

        resultingAvailabilityState.Recalculations.Should().ContainSingle(s =>
            s.Booking.Reference == "2" && s.Action == AvailabilityUpdateAction.SetToOrphaned);
        resultingAvailabilityState.AvailableSlots.Should().HaveCount(17);
    }

    [Fact]
    public async Task DeletesProvisionalAppointments()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", avStatus: "Supported", status: "Booked", creationOrder: 1),
            TestBooking("2", "Green", "09:10", status: "Provisional", creationOrder: 2)
        };

        var sessions = new List<SessionInstance> { TestSession("10:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));

        resultingAvailabilityState.Recalculations.Should().Contain(s =>
            s.Booking.Reference == "1" && s.Action == AvailabilityUpdateAction.SetToOrphaned);
        resultingAvailabilityState.Recalculations.Should().Contain(s =>
            s.Booking.Reference == "2" && s.Action == AvailabilityUpdateAction.ProvisionalToDelete);
        resultingAvailabilityState.AvailableSlots.Should().HaveCount(12);
    }

    [Fact]
    public async Task PrioritisesAppointmentsByCreatedDate()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", "09:30", creationOrder: 3),
            TestBooking("2", "Green", "09:30", creationOrder: 1),
            TestBooking("3", "Green", "09:30", creationOrder: 2)
        };

        var sessions = new List<SessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));
        resultingAvailabilityState.Recalculations.Should().ContainSingle(r =>
            r.Booking.Reference == "2" && r.Action == AvailabilityUpdateAction.SetToSupported);
        resultingAvailabilityState.AvailableSlots.Should().HaveCount(17);
    }
}
