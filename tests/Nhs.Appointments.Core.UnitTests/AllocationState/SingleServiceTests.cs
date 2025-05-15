namespace Nhs.Appointments.Core.UnitTests.AllocationState;

public class SingleServiceTests : AllocationStateServiceTestBase
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

        var resultingAllocationState = await Sut.BuildAllocation(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 30, 0));

        resultingAllocationState.SupportedBookingReferences.Should().BeEquivalentTo(bookings.Select(b => b.Reference));
        resultingAllocationState.AvailableSlots.Should().HaveCount(15);
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

        var resultingAllocationState = await Sut.BuildAllocation(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 30, 0));

        // resultingAllocationState.Recalculations.Should().HaveCount(3);
        resultingAllocationState.SupportedBookingReferences.Should().BeEquivalentTo(bookings.Select(b => b.Reference));
        resultingAllocationState.AvailableSlots.Should().HaveCount(15);
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

        var resultingAllocationState = await Sut.BuildAllocation(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 30, 0));

        // resultingAllocationState.Recalculations.Should().ContainSingle(s =>
        //     s.Booking.Reference == "2" && s.Action == AvailabilityUpdateAction.SetToOrphaned);
        resultingAllocationState.SupportedBookingReferences.Should().HaveCount(1);
        resultingAllocationState.AvailableSlots.Should().HaveCount(17);
    }

    [Fact]
    public async Task DeletesProvisionalAppointments()
    {
        var utcNow = DateTime.UtcNow;
        base.TimeProvider.Setup(x => x.GetUtcNow()).Returns(utcNow);
        
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", avStatus: "Supported", status: "Booked", creationOrder: 1),
            TestBooking("2", "Green", "09:10", status: "Provisional", creationOrder: 2, creationDate: utcNow.AddMinutes(-3))
        };

        var sessions = new List<SessionInstance> { TestSession("10:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAllocationState = await Sut.BuildAllocation(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0));

        // resultingAllocationState.Recalculations.Should().Contain(s =>
        //     s.Booking.Reference == "1" && s.Action == AvailabilityUpdateAction.SetToOrphaned);
        // resultingAllocationState.Recalculations.Should().Contain(s =>
        //     s.Booking.Reference == "2" && s.Action == AvailabilityUpdateAction.ProvisionalToDelete);
        resultingAllocationState.SupportedBookingReferences.Should().BeEmpty();
        resultingAllocationState.AvailableSlots.Should().HaveCount(12);
    }

    [Fact]
    public async Task ExpiredProvisionalAppointments_NotDeleted()
    {
        var utcNow = DateTime.UtcNow;
        base.TimeProvider.Setup(x => x.GetUtcNow()).Returns(utcNow);
        
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", avStatus: "Supported", status: "Booked", creationOrder: 1),
            TestBooking("2", "Green", "09:10", status: "Provisional", creationOrder: 2, creationDate: utcNow.AddMinutes(-8))
        };

        var sessions = new List<SessionInstance> { TestSession("10:00", "12:00", ["Green"]) };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAllocationState = await Sut.BuildAllocation(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0));

        // resultingAllocationState.Recalculations.Should().Contain(s =>
        //     s.Booking.Reference == "1" && s.Action == AvailabilityUpdateAction.SetToOrphaned);
        //
        //if a provisional booking has expired, we don't need to include it in our availability state
        //it will eventually be deleted up by the expired process
        // resultingAllocationState.Recalculations.Should().NotContain(s =>
        //     s.Booking.Reference == "2" && s.Action == AvailabilityUpdateAction.ProvisionalToDelete);
        resultingAllocationState.SupportedBookingReferences.Should().BeEmpty();
        resultingAllocationState.AvailableSlots.Should().HaveCount(12);
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

        var resultingAllocationState = await Sut.BuildAllocation(MockSite, new DateTime(2025, 1, 1, 9, 30, 0), new DateTime(2025, 1, 1, 9, 30, 0));

        resultingAllocationState.SupportedBookingReferences.Should().ContainSingle(b => b == "2");
        // resultingAllocationState.Recalculations.Should().ContainSingle(r =>
        //     r.Booking.Reference == "2" && r.Action == AvailabilityUpdateAction.SetToSupported);
        resultingAllocationState.AvailableSlots.Should().HaveCount(17);
    }
}
