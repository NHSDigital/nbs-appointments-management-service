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

        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 7));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 8));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 9));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 10));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 11));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 12));
    }

    /// <summary>
    ///     Prove that greedy model can in some cases increase utilisation where the SingleService code would not
    /// </summary>
    [Fact]
    public async Task MultipleServices_IncreasedUtilisation_1()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "C", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "C", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "C", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "F", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "G", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "E", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 6),
            TestBooking("11", "C", new DateOnly(2025, 1, 19), "09:30", avStatus: "Orphaned", creationOrder: 11),
            TestBooking("12", "C", new DateOnly(2025, 1, 19), "09:30", avStatus: "Orphaned", creationOrder: 12),
            TestBooking("13", "C", new DateOnly(2025, 1, 19), "09:30", avStatus: "Orphaned", creationOrder: 13),
            TestBooking("14", "F", new DateOnly(2025, 1, 19), "09:30", avStatus: "Orphaned", creationOrder: 14),
            TestBooking("15", "G", new DateOnly(2025, 1, 19), "09:30", avStatus: "Orphaned", creationOrder: 15),
            TestBooking("16", "E", new DateOnly(2025, 1, 19), "09:30", avStatus: "Orphaned", creationOrder: 16),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 13), "09:00", "09:40", ["B", "C", "D", "E", "F", "G"], capacity: 3,
                internalSessionId: Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0")),
            TestSession(new DateOnly(2025, 1, 13), "09:00", "09:40", ["A", "C", "D"], capacity: 5,
                internalSessionId: Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95")),
            TestSession(new DateOnly(2025, 1, 19), "09:00", "09:40", ["B", "C", "D", "E", "F", "G"], capacity: 3,
                internalSessionId: Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4")),
            TestSession(new DateOnly(2025, 1, 19), "09:00", "09:40", ["A", "C", "D"], capacity: 5,
                internalSessionId: Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742")),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var weekSummary = await Sut.GetWeekSummary(MockSite, new DateOnly(2025, 1, 13));

        weekSummary.DaySummaries.Should().HaveCount(7);

        weekSummary.MaximumCapacity.Should().Be(64);
        weekSummary.RemainingCapacity.Should().Be(52);
        weekSummary.TotalBooked.Should().Be(12);
        weekSummary.TotalOrphaned.Should().Be(0);
        weekSummary.TotalCancelled.Should().Be(0);

        var expectedSessionSummary1 = new List<SessionSummary>
        {
            new()
            {
                Id = Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0"),
                From = new DateTime(2025, 1, 13, 9, 0, 0),
                Until = new DateTime(2025, 1, 13, 9, 40, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 3,
                ServiceBookings = new Dictionary<string, int>
                {
                    { "B", 0 },
                    { "C", 0 },
                    { "D", 0 },
                    { "E", 1 },
                    { "F", 1 },
                    { "G", 1 },
                }
            },
            new()
            {
                Id = Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95"),
                From = new DateTime(2025, 1, 13, 9, 0, 0),
                Until = new DateTime(2025, 1, 13, 9, 40, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                ServiceBookings = new Dictionary<string, int> { { "A", 0 }, { "C", 3 }, { "D", 0 } }
            }
        };

        var daySummaryAffected1 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 13));

        daySummaryAffected1.Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 13), expectedSessionSummary1)
            {
                MaximumCapacity = 32,
                RemainingCapacity = 26,
                TotalBooked = 6,
                TotalOrphaned = 0,
                TotalCancelled = 0
            });

        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 14));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 15));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 16));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 17));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 18));

        var expectedSessionSummary2 = new List<SessionSummary>
        {
            new()
            {
                Id = Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4"),
                From = new DateTime(2025, 1, 19, 9, 0, 0),
                Until = new DateTime(2025, 1, 19, 9, 40, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 3,
                ServiceBookings = new Dictionary<string, int>
                {
                    { "B", 0 },
                    { "C", 0 },
                    { "D", 0 },
                    { "E", 1 },
                    { "F", 1 },
                    { "G", 1 },
                }
            },
            new()
            {
                Id = Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742"),
                From = new DateTime(2025, 1, 19, 9, 0, 0),
                Until = new DateTime(2025, 1, 19, 9, 40, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                ServiceBookings = new Dictionary<string, int> { { "A", 0 }, { "C", 3 }, { "D", 0 } }
            }
        };

        var daySummaryAffected2 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 19));

        daySummaryAffected2.Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 19), expectedSessionSummary2)
            {
                MaximumCapacity = 32,
                RemainingCapacity = 26,
                TotalBooked = 6,
                TotalOrphaned = 0,
                TotalCancelled = 0
            });
    }

    /// <summary>
    ///     Prove that greedy model can in some cases increase utilisation where the SingleService code would not
    /// </summary>
    [Fact]
    public async Task MultipleServices_IncreasedUtilisation_2()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "C", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "C", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "C", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "B", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "B", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "B", new DateOnly(2025, 1, 13), avStatus: "Orphaned", creationOrder: 6),
            TestBooking("11", "C", new DateOnly(2025, 1, 19), avStatus: "Orphaned", creationOrder: 11),
            TestBooking("12", "C", new DateOnly(2025, 1, 19), avStatus: "Orphaned", creationOrder: 12),
            TestBooking("13", "C", new DateOnly(2025, 1, 19), avStatus: "Orphaned", creationOrder: 13),
            TestBooking("14", "B", new DateOnly(2025, 1, 19), avStatus: "Orphaned", creationOrder: 14),
            TestBooking("15", "B", new DateOnly(2025, 1, 19), avStatus: "Orphaned", creationOrder: 15),
            TestBooking("16", "B", new DateOnly(2025, 1, 19), avStatus: "Orphaned", creationOrder: 16),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 13), "09:00", "09:40", ["B", "C", "D"], capacity: 3,
                internalSessionId: Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0")),
            TestSession(new DateOnly(2025, 1, 13), "09:00", "09:40", ["A", "C", "D"], capacity: 5,
                internalSessionId: Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95")),
            TestSession(new DateOnly(2025, 1, 19), "09:00", "09:40", ["B", "C", "D"], capacity: 3,
                internalSessionId: Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4")),
            TestSession(new DateOnly(2025, 1, 19), "09:00", "09:40", ["A", "C", "D"], capacity: 5,
                internalSessionId: Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742")),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var weekSummary = await Sut.GetWeekSummary(MockSite, new DateOnly(2025, 1, 13));

        weekSummary.DaySummaries.Should().HaveCount(7);

        weekSummary.MaximumCapacity.Should().Be(64);
        weekSummary.RemainingCapacity.Should().Be(52);
        weekSummary.TotalBooked.Should().Be(12);
        weekSummary.TotalOrphaned.Should().Be(0);
        weekSummary.TotalCancelled.Should().Be(0);

        var expectedSessionSummary1 = new List<SessionSummary>
        {
            new()
            {
                Id = Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0"),
                From = new DateTime(2025, 1, 13, 9, 0, 0),
                Until = new DateTime(2025, 1, 13, 9, 40, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 3,
                ServiceBookings = new Dictionary<string, int> { { "B", 3 }, { "C", 0 }, { "D", 0 } }
            },
            new()
            {
                Id = Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95"),
                From = new DateTime(2025, 1, 13, 9, 0, 0),
                Until = new DateTime(2025, 1, 13, 9, 40, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                ServiceBookings = new Dictionary<string, int> { { "A", 0 }, { "C", 3 }, { "D", 0 } }
            }
        };

        var daySummaryAffected1 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 13));

        daySummaryAffected1.Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 13), expectedSessionSummary1)
            {
                MaximumCapacity = 32,
                RemainingCapacity = 26,
                TotalBooked = 6,
                TotalOrphaned = 0,
                TotalCancelled = 0
            });

        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 14));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 15));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 16));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 17));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 18));

        var expectedSessionSummary2 = new List<SessionSummary>
        {
            new()
            {
                Id = Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4"),
                From = new DateTime(2025, 1, 19, 9, 0, 0),
                Until = new DateTime(2025, 1, 19, 9, 40, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 3,
                ServiceBookings = new Dictionary<string, int> { { "B", 3 }, { "C", 0 }, { "D", 0 } }
            },
            new()
            {
                Id = Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742"),
                From = new DateTime(2025, 1, 19, 9, 0, 0),
                Until = new DateTime(2025, 1, 19, 9, 40, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                ServiceBookings = new Dictionary<string, int> { { "A", 0 }, { "C", 3 }, { "D", 0 } }
            }
        };

        var daySummaryAffected2 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 19));

        daySummaryAffected2.Should()
            .BeEquivalentTo(new DaySummary(new DateOnly(2025, 1, 19), expectedSessionSummary2)
            {
                MaximumCapacity = 32,
                RemainingCapacity = 26,
                TotalBooked = 6,
                TotalOrphaned = 0,
                TotalCancelled = 0
            });
    }


    //
    // /// <summary>
    // /// Prove that greedy model isn't always optimal, and can lead to loss in utilisation
    // /// </summary>
    // [Fact]
    // public async Task MultipleServices_LostUtilisation_1()
    // {
    //     var bookings = new List<Booking>
    //     {
    //         TestBooking("1", "C", avStatus: "Orphaned", creationOrder: 1),
    //         TestBooking("2", "C", avStatus: "Orphaned", creationOrder: 2),
    //         TestBooking("3", "C", avStatus: "Orphaned", creationOrder: 3),
    //         TestBooking("4", "F", avStatus: "Orphaned", creationOrder: 4),
    //         TestBooking("5", "G", avStatus: "Orphaned", creationOrder: 5),
    //         TestBooking("6", "E", avStatus: "Orphaned", creationOrder: 6),
    //     };
    //
    //     var sessions = new List<SessionInstance>
    //     {
    //         TestSession("09:00", "09:10", ["B", "C", "D", "E", "F", "G"], capacity: 3),
    //         TestSession("09:00", "09:10", ["A", "C", "D", "U", "Z", "H", "I", "J"], capacity: 3),
    //     };
    //
    //     SetupAvailabilityAndBookings(bookings, sessions);
    //         
    //     var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();
    //
    //     // Bookings 1, 2, 3 should be supported
    //     recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
    //         .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3");
    //
    //     // Bookings 4, 5, 6 should not be, due to non-optimal greedy algorithm
    //     recalculations.Should().NotContain(r => r.Booking.Reference == "4");
    //     recalculations.Should().NotContain(r => r.Booking.Reference == "5");
    //     recalculations.Should().NotContain(r => r.Booking.Reference == "6");
    // }
    //
    // /// <summary>
    // /// Prove that greedy model isn't always optimal, and can lead to loss in utilisation
    // /// This test proves a scenario where just selecting the first available slot would actually have been preferable over the alphabetical ordering
    // /// </summary>
    // [Fact]
    // public async Task MultipleServices_LostUtilisation_2()
    // {
    //     var bookings = new List<Booking>
    //     {
    //         TestBooking("1", "C", avStatus: "Orphaned", creationOrder: 1),
    //         TestBooking("2", "C", avStatus: "Orphaned", creationOrder: 2),
    //         TestBooking("3", "C", avStatus: "Orphaned", creationOrder: 3),
    //         TestBooking("4", "B", avStatus: "Orphaned", creationOrder: 4),
    //         TestBooking("5", "B", avStatus: "Orphaned", creationOrder: 5),
    //         TestBooking("6", "B", avStatus: "Orphaned", creationOrder: 6),
    //     };
    //
    //     var sessions = new List<SessionInstance>
    //     {
    //         TestSession("09:00", "09:10", ["D", "C", "E"], capacity: 3),
    //         TestSession("09:00", "09:10", ["B", "C", "D"], capacity: 3),
    //     };
    //
    //     SetupAvailabilityAndBookings(bookings, sessions);
    //         
    //     var recalculations = (await Sut.BuildRecalculations(MockSite, new DateTime(2025, 1, 1, 9, 0, 0), new DateTime(2025, 1, 1, 9, 10, 0))).ToList();
    //
    //     // Bookings 1, 2, 3 should be supported
    //     recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
    //         .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3");
    //
    //     // Bookings 4, 5, 6 should not be, due to non-optimal greedy algorithm
    //     recalculations.Should().NotContain(r => r.Booking.Reference == "4");
    //     recalculations.Should().NotContain(r => r.Booking.Reference == "5");
    //     recalculations.Should().NotContain(r => r.Booking.Reference == "6");
    // }
}

public static class Assertions
{
    public static void AssertEmptySessionSummariesOnDate(this IEnumerable<DaySummary> daySummaries, DateOnly date)
    {
        daySummaries.Single(x => x.Date == date).Should().BeEquivalentTo(new DaySummary(date, []));
    }
}
