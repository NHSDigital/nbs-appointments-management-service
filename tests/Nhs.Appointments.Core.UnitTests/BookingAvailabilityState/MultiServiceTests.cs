namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class MultiServiceTests : BookingAvailabilityStateServiceTestBase
{
    [Fact]
    public async Task MultipleServicesByCreatedDate()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Blue", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Green", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Green", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("5", "Blue", avStatus: "Orphaned", creationOrder: 7),
            TestBooking("6", "Green", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("7", "Blue", avStatus: "Orphaned", creationOrder: 5)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 2),
            TestSession("09:00", "10:00", ["Green"], capacity: 1),
            TestSession("09:00", "10:00", ["Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);
            
        var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        // Bookings 1, 2, 3, 6 and 7 should be supported
        recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "6", "7");

        // Bookings 4 and 5 should not be, because they were created after 6 and 7
        recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        recalculations.Should().NotContain(r => r.Booking.Reference == "5");

        recalculations.Should().HaveCount(5);
        recalculations.Select(b => b.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "6", "7");
    }

    [Fact]
    public async Task TheBestFitProblem()
    {
        // See: https://app.mural.co/t/nhsdigital8118/m/nhsdigital8118/1737058837342/35b45e0418f8661f6ad19efed4bf3fd0cc9bb3d5

        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Green", "Orange"], capacity: 1),
            TestSession("09:00", "10:00", ["Orange", "Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();

        // Bookings 1 and 2 can be booked
        recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2");

        // Booking 3 could have been booked if we rejuggled appointments, but currently we do not
        recalculations.Select(b => b.Booking.Reference).Should().NotContain(r => r == "3");

        recalculations.Should().HaveCount(2);
        recalculations.Select(b => b.Booking.Reference).Should().BeEquivalentTo("1", "2");
    }
}
