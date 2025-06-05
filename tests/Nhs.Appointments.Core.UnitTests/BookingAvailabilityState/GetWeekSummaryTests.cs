namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class GetWeekSummaryTests : BookingAvailabilityStateServiceTestBase
{
    [Fact]
    public async Task MultipleServices_ByCreatedDate()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", new DateOnly(2025, 1, 6), avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Blue", new DateOnly(2025, 1, 6), avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Green", new DateOnly(2025, 1, 6), avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Green", new DateOnly(2025, 1, 6), avStatus: "Orphaned", creationOrder: 6),
            TestBooking("5", "Blue", new DateOnly(2025, 1, 6), avStatus: "Orphaned", creationOrder: 7),
            TestBooking("6", "Green", new DateOnly(2025, 1, 6), avStatus: "Orphaned", creationOrder: 4),
            TestBooking("7", "Blue", new DateOnly(2025, 1, 6), avStatus: "Orphaned", creationOrder: 5)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green", "Blue"], capacity: 2,
                internalSessionId: Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742")),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green"], capacity: 1,
                internalSessionId: Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0")),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Blue"], capacity: 1,
                internalSessionId: Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4")),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Blue"], capacity: 1,
                internalSessionId: Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95"))
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var weekSummary = await Sut.GetWeekSummary(MockSite, new DateOnly(2025, 1, 6));

        weekSummary.DaySummaries.Should().HaveCount(7);

        weekSummary.MaximumCapacity.Should().Be(30);
        weekSummary.RemainingCapacity.Should().Be(25);
        weekSummary.TotalBooked.Should().Be(7);
        weekSummary.TotalOrphaned.Should().Be(2);
        weekSummary.TotalCancelled.Should().Be(0);

        var expectedSessionSummary = new List<SessionSummary>
        {
            new()
            {
                Id = Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742"),
                From = new DateTime(2025, 1, 6, 9, 0, 0),
                Until = new DateTime(2025, 1, 6, 10, 0, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 2,
                ServiceBookings = new Dictionary<string, int> { { "Green", 1 }, { "Blue", 1 } }
            },
            new()
            {
                Id = Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0"),
                From = new DateTime(2025, 1, 6, 9, 0, 0),
                Until = new DateTime(2025, 1, 6, 10, 0, 0),
                MaximumCapacity = 6,
                SlotLength = 10,
                Capacity = 1,
                ServiceBookings = new Dictionary<string, int> { { "Green", 1 } }
            },
            new()
            {
                Id = Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4"),
                From = new DateTime(2025, 1, 6, 9, 0, 0),
                Until = new DateTime(2025, 1, 6, 10, 0, 0),
                MaximumCapacity = 6,
                SlotLength = 10,
                Capacity = 1,
                ServiceBookings = new Dictionary<string, int> { { "Blue", 1 } }
            },
            new()
            {
                Id = Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95"),
                From = new DateTime(2025, 1, 6, 9, 0, 0),
                Until = new DateTime(2025, 1, 6, 10, 0, 0),
                MaximumCapacity = 6,
                SlotLength = 10,
                Capacity = 1,
                ServiceBookings = new Dictionary<string, int> { { "Blue", 1 } }
            }
        };

        List<SessionSummary> emptySummaries = [];

        var daySummaryAffected = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 6));

        daySummaryAffected.Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 6), expectedSessionSummary)
            {
                MaximumCapacity = 30,
                RemainingCapacity = 25,
                TotalBooked = 7,
                TotalOrphaned = 2,
                TotalCancelled = 0
            });

        weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 7)).Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 7), emptySummaries));

        weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 8)).Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 8), emptySummaries));

        weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 9)).Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 9), emptySummaries));

        weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 10)).Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 10), emptySummaries));

        weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 11)).Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 11), emptySummaries));

        weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 12)).Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 12), emptySummaries));
    }
}
