using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;

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
            TestBooking("7", "Blue", new DateOnly(2025, 1, 6), avStatus: "Orphaned", creationOrder: 5),
            TestBooking("8", "Blue", new DateOnly(2025, 1, 6), status: "Cancelled", creationOrder: 8),
            TestBooking("9", "Green", new DateOnly(2025, 1, 6), status: "Cancelled", creationOrder: 9),
            TestBooking("10", "Pink", new DateOnly(2025, 1, 6), duration: 60, avStatus: "Orphaned", creationOrder: 10),
            
            TestBooking("11", "Blue", new DateOnly(2025, 1, 7), avStatus: "Orphaned", creationOrder: 11),
            TestBooking("12", "Blue", new DateOnly(2025, 1, 7), avStatus: "Orphaned", creationOrder: 12),
            TestBooking("13", "Green", new DateOnly(2025, 1, 7), avStatus: "Orphaned", creationOrder: 13),
            TestBooking("14", "Green", new DateOnly(2025, 1, 7), avStatus: "Orphaned", creationOrder: 16),
            TestBooking("15", "Blue", new DateOnly(2025, 1, 7), avStatus: "Orphaned", creationOrder: 17),
            TestBooking("16", "Green", new DateOnly(2025, 1, 7), avStatus: "Orphaned", creationOrder: 14),
            TestBooking("17", "Blue", new DateOnly(2025, 1, 7), avStatus: "Orphaned", creationOrder: 15),
            TestBooking("18", "Blue", new DateOnly(2025, 1, 7), status: "Cancelled", creationOrder: 18),
            TestBooking("19", "Green", new DateOnly(2025, 1, 7), status: "Cancelled", creationOrder: 19)
        };

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green", "Blue", "Red"], capacity: 2,
                internalSessionId: Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742")),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Green"], capacity: 1,
                internalSessionId: Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0")),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Blue"], capacity: 1,
                internalSessionId: Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4")),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Blue", "Red", "Purple"], capacity: 1,
                internalSessionId: Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95")),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "15:00", ["Yellow"], capacity: 5, slotLength: 30,
                internalSessionId: Guid.Parse("df7c3571-bfab-4c88-8ae3-7a4b1622bddb")),
            TestSession(new DateOnly(2025, 1, 6), "09:00", "10:00", ["Pink"], capacity: 1, slotLength: 60,
                internalSessionId: Guid.Parse("927588e1-09c2-4e9d-8dfa-61f51be853bd")),
            
            TestSession(new DateOnly(2025, 1, 7), "09:00", "10:00", ["Green", "Blue", "Red"], capacity: 2,
                internalSessionId: Guid.Parse("a9907d84-a0e3-41d4-ae49-bed6c23d9742")),
            TestSession(new DateOnly(2025, 1, 7), "09:00", "10:00", ["Green"], capacity: 1,
                internalSessionId: Guid.Parse("acff90d1-fe20-477e-af02-dac209dd86c0")),
            TestSession(new DateOnly(2025, 1, 7), "09:00", "10:00", ["Blue"], capacity: 1,
                internalSessionId: Guid.Parse("ac1b5938-922d-45f0-b301-5f9156bb0de4")),
            TestSession(new DateOnly(2025, 1, 7), "09:00", "10:00", ["Blue", "Red", "Purple"], capacity: 1,
                internalSessionId: Guid.Parse("a28f5107-3972-4a35-bd35-b7476442ad95")),
            TestSession(new DateOnly(2025, 1, 7), "09:00", "15:00", ["Yellow"], capacity: 5, slotLength: 30,
                internalSessionId: Guid.Parse("af7c3571-bfab-4c88-8ae3-7a4b1622bddb"))
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var weekSummary = await Sut.GetWeekSummary(MockSite, new DateOnly(2025, 1, 6));

        weekSummary.DaySummaries.Should().HaveCount(7);

        weekSummary.MaximumCapacity.Should().Be(181);
        weekSummary.TotalRemainingCapacity.Should().Be(170);

        weekSummary.TotalSupportedAppointments.Should().Be(11);
        weekSummary.TotalSupportedAppointmentsByService.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            { "Blue", 6 }, { "Green", 4 }, { "Pink", 1 }, { "Red", 0 }, { "Yellow", 0 }, { "Purple", 0 }
        });

        weekSummary.TotalOrphanedAppointments.Should().Be(4);
        weekSummary.TotalOrphanedAppointmentsByService.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            { "Blue", 2 }, { "Green", 2 }
        });

        weekSummary.TotalCancelledAppointments.Should().Be(4);
        weekSummary.TotalCancelledAppointmentsByService.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            { "Blue", 2 }, { "Green", 2 }
        });
        
        weekSummary.TotalRemainingCapacityByService.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            { "Blue", 40 }, { "Green", 30 }, { "Red", 30 }, { "Yellow", 120 }, { "Purple", 10 }, { "Pink", 0 }
        });

        var expectedSessionSummaries1 = new List<SessionAvailabilitySummary>
        {
            new()
            {
                Id = Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742"),
                UkStartDatetime = new DateTime(2025, 1, 6, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 6, 10, 0, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 2,
                TotalSupportedAppointmentsByService =
                    new Dictionary<string, int> { { "Green", 1 }, { "Blue", 1 }, { "Red", 0 } },
                TotalRemainingCapacityByService = 
                    new Dictionary<string, int> { { "Green", 10 }, { "Blue", 10 }, { "Red", 10 } },
            },
            new()
            {
                Id = Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0"),
                UkStartDatetime = new DateTime(2025, 1, 6, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 6, 10, 0, 0),
                MaximumCapacity = 6,
                SlotLength = 10,
                Capacity = 1,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "Green", 1 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "Green", 5 } },
            },
            new()
            {
                Id = Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4"),
                UkStartDatetime = new DateTime(2025, 1, 6, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 6, 10, 0, 0),
                MaximumCapacity = 6,
                SlotLength = 10,
                Capacity = 1,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "Blue", 1 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "Blue", 5 } },
            },
            new()
            {
                Id = Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95"),
                UkStartDatetime = new DateTime(2025, 1, 6, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 6, 10, 0, 0),
                MaximumCapacity = 6,
                SlotLength = 10,
                Capacity = 1,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "Blue", 1 }, { "Red", 0 }, { "Purple", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "Blue", 5 }, { "Red", 5 }, { "Purple", 5 } },
            },
            new()
            {
                Id = Guid.Parse("df7c3571-bfab-4c88-8ae3-7a4b1622bddb"),
                UkStartDatetime = new DateTime(2025, 1, 6, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 6, 15, 0, 0),
                MaximumCapacity = 60,
                SlotLength = 30,
                Capacity = 5,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "Yellow", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "Yellow", 60 } }
            },
            new()
            {
                Id = Guid.Parse("927588e1-09c2-4e9d-8dfa-61f51be853bd"),
                UkStartDatetime = new DateTime(2025, 1, 6, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 6, 10, 0, 0),
                MaximumCapacity = 1,
                SlotLength = 60,
                Capacity = 1,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "Pink", 1 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "Pink", 0 } },
            }
        };

        var daySummaryAffected1 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 6));

        daySummaryAffected1.Date.Should().Be(new DateOnly(2025, 1, 6));
        daySummaryAffected1.MaximumCapacity.Should().Be(91);
        
        daySummaryAffected1.TotalRemainingCapacity.Should().Be(85);
        daySummaryAffected1.TotalRemainingCapacityByService.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            { "Blue", 20 }, { "Green", 15 }, { "Red", 15 }, { "Yellow", 60 }, { "Purple", 5 }, { "Pink", 0 }
        });
        
        daySummaryAffected1.TotalSupportedAppointments.Should().Be(6);
        daySummaryAffected1.TotalOrphanedAppointments.Should().Be(2);
        daySummaryAffected1.TotalCancelledAppointments.Should().Be(2);

        daySummaryAffected1.SessionSummaries.Should().BeEquivalentTo(expectedSessionSummaries1);
        
        var expectedSessionSummaries2 = new List<SessionAvailabilitySummary>
        {
            new()
            {
                Id = Guid.Parse("a9907d84-a0e3-41d4-ae49-bed6c23d9742"),
                UkStartDatetime = new DateTime(2025, 1, 7, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 7, 10, 0, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 2,
                TotalSupportedAppointmentsByService =
                    new Dictionary<string, int> { { "Green", 1 }, { "Blue", 1 }, { "Red", 0 } },
                TotalRemainingCapacityByService = 
                    new Dictionary<string, int> { { "Green", 10 }, { "Blue", 10 }, { "Red", 10 } },
            },
            new()
            {
                Id = Guid.Parse("acff90d1-fe20-477e-af02-dac209dd86c0"),
                UkStartDatetime = new DateTime(2025, 1, 7, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 7, 10, 0, 0),
                MaximumCapacity = 6,
                SlotLength = 10,
                Capacity = 1,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "Green", 1 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "Green", 5 } },
            },
            new()
            {
                Id = Guid.Parse("ac1b5938-922d-45f0-b301-5f9156bb0de4"),
                UkStartDatetime = new DateTime(2025, 1, 7, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 7, 10, 0, 0),
                MaximumCapacity = 6,
                SlotLength = 10,
                Capacity = 1,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "Blue", 1 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "Blue", 5 } },
            },
            new()
            {
                Id = Guid.Parse("a28f5107-3972-4a35-bd35-b7476442ad95"),
                UkStartDatetime = new DateTime(2025, 1, 7, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 7, 10, 0, 0),
                MaximumCapacity = 6,
                SlotLength = 10,
                Capacity = 1,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "Blue", 1 }, { "Red", 0 }, { "Purple", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "Blue", 5 }, { "Red", 5 }, { "Purple", 5 } },
            },
            new()
            {
                Id = Guid.Parse("af7c3571-bfab-4c88-8ae3-7a4b1622bddb"),
                UkStartDatetime = new DateTime(2025, 1, 7, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 7, 15, 0, 0),
                MaximumCapacity = 60,
                SlotLength = 30,
                Capacity = 5,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "Yellow", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "Yellow", 60 } }
            }
        };

        var daySummaryAffected2 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 7));

        daySummaryAffected2.Date.Should().Be(new DateOnly(2025, 1, 7));
        daySummaryAffected2.MaximumCapacity.Should().Be(90);
        
        daySummaryAffected2.TotalRemainingCapacity.Should().Be(85);
        daySummaryAffected2.TotalRemainingCapacityByService.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            { "Blue", 20 }, { "Green", 15 }, { "Red", 15 }, { "Yellow", 60 }, { "Purple", 5 }
        });
        
        daySummaryAffected2.TotalSupportedAppointments.Should().Be(5);
        daySummaryAffected2.TotalOrphanedAppointments.Should().Be(2);
        daySummaryAffected2.TotalCancelledAppointments.Should().Be(2);

        daySummaryAffected2.SessionSummaries.Should().BeEquivalentTo(expectedSessionSummaries2);

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

        var sessions = new List<LinkedSessionInstance>
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
        weekSummary.TotalRemainingCapacity.Should().Be(52);
        weekSummary.TotalSupportedAppointments.Should().Be(12);
        weekSummary.TotalOrphanedAppointments.Should().Be(0);

        var expectedSessionAvailabilitySummary1 = new List<SessionAvailabilitySummary>
        {
            new()
            {
                Id = Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0"),
                UkStartDatetime = new DateTime(2025, 1, 13, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 13, 9, 40, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 3,
                TotalSupportedAppointmentsByService = new Dictionary<string, int>
                {
                    { "B", 0 },
                    { "C", 0 },
                    { "D", 0 },
                    { "E", 1 },
                    { "F", 1 },
                    { "G", 1 },
                },
                TotalRemainingCapacityByService = new Dictionary<string, int>
                {
                    { "B", 9 },
                    { "C", 9 },
                    { "D", 9 },
                    { "E", 9 },
                    { "F", 9 },
                    { "G", 9 },
                }
            },
            new()
            {
                Id = Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95"),
                UkStartDatetime = new DateTime(2025, 1, 13, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 13, 9, 40, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "A", 0 }, { "C", 3 }, { "D", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "A", 17 }, { "C", 17 }, { "D", 17 } },
            }
        };

        var daySummaryAffected1 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 13));
        
        daySummaryAffected1.Date.Should().Be(new DateOnly(2025, 1, 13));
        daySummaryAffected1.MaximumCapacity.Should().Be(32);
        daySummaryAffected1.TotalRemainingCapacity.Should().Be(26);
        daySummaryAffected1.TotalSupportedAppointments.Should().Be(6);
        daySummaryAffected1.TotalOrphanedAppointments.Should().Be(0);
        daySummaryAffected1.TotalCancelledAppointments.Should().Be(0);
        daySummaryAffected1.SessionSummaries.Should().BeEquivalentTo(expectedSessionAvailabilitySummary1);

        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 14));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 15));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 16));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 17));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 18));

        var expectedSessionAvailabilitySummary2 = new List<SessionAvailabilitySummary>
        {
            new()
            {
                Id = Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4"),
                UkStartDatetime = new DateTime(2025, 1, 19, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 19, 9, 40, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 3,
                TotalSupportedAppointmentsByService = new Dictionary<string, int>
                {
                    { "B", 0 },
                    { "C", 0 },
                    { "D", 0 },
                    { "E", 1 },
                    { "F", 1 },
                    { "G", 1 },
                },
                TotalRemainingCapacityByService = new Dictionary<string, int>
                {
                    { "B", 9 },
                    { "C", 9 },
                    { "D", 9 },
                    { "E", 9 },
                    { "F", 9 },
                    { "G", 9 },
                },
            },
            new()
            {
                Id = Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742"),
                UkStartDatetime = new DateTime(2025, 1, 19, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 19, 9, 40, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "A", 0 }, { "C", 3 }, { "D", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "A", 17 }, { "C", 17 }, { "D", 17 } }
            }
        };

        var daySummaryAffected2 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 19));
        
        daySummaryAffected2.Date.Should().Be(new DateOnly(2025, 1, 19));
        daySummaryAffected2.MaximumCapacity.Should().Be(32);
        daySummaryAffected2.TotalRemainingCapacity.Should().Be(26);
        daySummaryAffected2.TotalSupportedAppointments.Should().Be(6);
        daySummaryAffected2.TotalOrphanedAppointments.Should().Be(0);
        daySummaryAffected2.TotalCancelledAppointments.Should().Be(0);
        daySummaryAffected2.SessionSummaries.Should().BeEquivalentTo(expectedSessionAvailabilitySummary2);
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

        var sessions = new List<LinkedSessionInstance>
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
        weekSummary.TotalRemainingCapacity.Should().Be(52);
        weekSummary.TotalSupportedAppointments.Should().Be(12);
        weekSummary.TotalOrphanedAppointments.Should().Be(0);

        var expectedSessionAvailabilitySummary1 = new List<SessionAvailabilitySummary>
        {
            new()
            {
                Id = Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0"),
                UkStartDatetime = new DateTime(2025, 1, 13, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 13, 9, 40, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 3,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "B", 3 }, { "C", 0 }, { "D", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "B", 9 }, { "C", 9 }, { "D", 9 } },
            },
            new()
            {
                Id = Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95"),
                UkStartDatetime = new DateTime(2025, 1, 13, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 13, 9, 40, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "A", 0 }, { "C", 3 }, { "D", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "A", 17 }, { "C", 17 }, { "D", 17 } },
            }
        };

        var daySummaryAffected1 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 13));
        
        daySummaryAffected1.Date.Should().Be(new DateOnly(2025, 1, 13));
        daySummaryAffected1.MaximumCapacity.Should().Be(32);
        daySummaryAffected1.TotalRemainingCapacity.Should().Be(26);
        daySummaryAffected1.TotalSupportedAppointments.Should().Be(6);
        daySummaryAffected1.TotalOrphanedAppointments.Should().Be(0);
        daySummaryAffected1.TotalCancelledAppointments.Should().Be(0);
        daySummaryAffected1.SessionSummaries.Should().BeEquivalentTo(expectedSessionAvailabilitySummary1);

        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 14));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 15));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 16));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 17));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 18));

        var expectedSessionAvailabilitySummary2 = new List<SessionAvailabilitySummary>
        {
            new()
            {
                Id = Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4"),
                UkStartDatetime = new DateTime(2025, 1, 19, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 19, 9, 40, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 3,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "B", 3 }, { "C", 0 }, { "D", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "B", 9 }, { "C", 9 }, { "D", 9 } },
            },
            new()
            {
                Id = Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742"),
                UkStartDatetime = new DateTime(2025, 1, 19, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 19, 9, 40, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "A", 0 }, { "C", 3 }, { "D", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "A", 17 }, { "C", 17 }, { "D", 17 } },
            }
        };

        var daySummaryAffected2 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 19));
        
        daySummaryAffected2.Date.Should().Be(new DateOnly(2025, 1, 19));
        daySummaryAffected2.MaximumCapacity.Should().Be(32);
        daySummaryAffected2.TotalRemainingCapacity.Should().Be(26);
        daySummaryAffected2.TotalSupportedAppointments.Should().Be(6);
        daySummaryAffected2.TotalOrphanedAppointments.Should().Be(0);
        daySummaryAffected2.TotalCancelledAppointments.Should().Be(0);
        daySummaryAffected2.SessionSummaries.Should().BeEquivalentTo(expectedSessionAvailabilitySummary2);
    }


    /// <summary>
    ///     Prove that greedy model isn't always optimal, and can lead to loss in utilisation
    /// </summary>
    [Fact]
    public async Task MultipleServices_LostUtilisation_1()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "C", new DateOnly(2025, 1, 14), avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "C", new DateOnly(2025, 1, 14), avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "C", new DateOnly(2025, 1, 14), avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "F", new DateOnly(2025, 1, 14), avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "G", new DateOnly(2025, 1, 14), avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "E", new DateOnly(2025, 1, 14), avStatus: "Orphaned", creationOrder: 6),
            TestBooking("11", "C", new DateOnly(2025, 1, 16), "09:30", avStatus: "Orphaned", creationOrder: 11),
            TestBooking("12", "C", new DateOnly(2025, 1, 16), "09:30", avStatus: "Orphaned", creationOrder: 12),
            TestBooking("13", "C", new DateOnly(2025, 1, 16), "09:30", avStatus: "Orphaned", creationOrder: 13),
            TestBooking("14", "F", new DateOnly(2025, 1, 16), "09:30", avStatus: "Orphaned", creationOrder: 14),
            TestBooking("15", "G", new DateOnly(2025, 1, 16), "09:30", avStatus: "Orphaned", creationOrder: 15),
            TestBooking("16", "E", new DateOnly(2025, 1, 16), "09:30", avStatus: "Orphaned", creationOrder: 16),
        };

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 14), "09:00", "09:50", ["B", "C", "D", "E", "F", "G"], capacity: 3,
                internalSessionId: Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0")),
            TestSession(new DateOnly(2025, 1, 14), "08:50", "09:30", ["A", "C", "D", "U", "Z", "H", "I", "J"],
                capacity: 5,
                internalSessionId: Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95")),
            TestSession(new DateOnly(2025, 1, 16), "08:00", "10:40", ["B", "C", "D", "E", "F", "G"], capacity: 3,
                internalSessionId: Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4")),
            TestSession(new DateOnly(2025, 1, 16), "09:00", "09:40", ["A", "C", "D", "U", "Z", "H", "I", "J"],
                capacity: 4,
                internalSessionId: Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742")),

            //extra unrelated data to pad test result
            TestSession(new DateOnly(2025, 1, 17), "09:00", "12:00", ["A", "C", "B", "D"], slotLength: 5, capacity: 2,
                internalSessionId: Guid.Parse("ae907d84-a0e3-41d4-ae49-bed6c23d9776")),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var weekSummary = await Sut.GetWeekSummary(MockSite, new DateOnly(2025, 1, 13));

        weekSummary.DaySummaries.Should().HaveCount(7);
        weekSummary.MaximumCapacity.Should().Be(171);
        weekSummary.TotalRemainingCapacity.Should().Be(165);

        //lost utilisation shows that 6/12 bookings are orphaned when they could have been allocated
        weekSummary.TotalSupportedAppointments.Should().Be(6);
        weekSummary.TotalOrphanedAppointments.Should().Be(6);

        var expectedSessionAvailabilitySummary1 = new List<SessionAvailabilitySummary>
        {
            new()
            {
                Id = Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0"),
                UkStartDatetime = new DateTime(2025, 1, 14, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 14, 9, 50, 0),
                MaximumCapacity = 15,
                SlotLength = 10,
                Capacity = 3,
                TotalSupportedAppointmentsByService = new Dictionary<string, int>
                {
                    { "B", 0 },
                    { "C", 3 },
                    { "D", 0 },
                    { "E", 0 },
                    { "F", 0 },
                    { "G", 0 },
                },
                TotalRemainingCapacityByService = new Dictionary<string, int>
                {
                    { "B", 12 },
                    { "C", 12 },
                    { "D", 12 },
                    { "E", 12 },
                    { "F", 12 },
                    { "G", 12 },
                }
            },
            new()
            {
                Id = Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95"),
                UkStartDatetime = new DateTime(2025, 1, 14, 8, 50, 0),
                UkEndDatetime = new DateTime(2025, 1, 14, 9, 30, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                TotalSupportedAppointmentsByService = new Dictionary<string, int>
                {
                    { "A", 0 },
                    { "C", 0 },
                    { "D", 0 },
                    { "U", 0 },
                    { "Z", 0 },
                    { "H", 0 },
                    { "I", 0 },
                    { "J", 0 },
                },
                TotalRemainingCapacityByService = new Dictionary<string, int>
                {
                    { "A", 20 },
                    { "C", 20 },
                    { "D", 20 },
                    { "U", 20 },
                    { "Z", 20 },
                    { "H", 20 },
                    { "I", 20 },
                    { "J", 20 },
                }                
            }
        };

        var daySummaryAffected1 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 14));

        daySummaryAffected1.Date.Should().Be(new DateOnly(2025, 1, 14));
        daySummaryAffected1.MaximumCapacity.Should().Be(35);
        daySummaryAffected1.TotalRemainingCapacity.Should().Be(32);
        daySummaryAffected1.TotalSupportedAppointments.Should().Be(3);
        daySummaryAffected1.TotalOrphanedAppointments.Should().Be(3);
        daySummaryAffected1.TotalCancelledAppointments.Should().Be(0);
        daySummaryAffected1.SessionSummaries.Should().BeEquivalentTo(expectedSessionAvailabilitySummary1);
        
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 13));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 15));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 18));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 19));

        var expectedSessionAvailabilitySummary2 = new List<SessionAvailabilitySummary>
        {
            new()
            {
                Id = Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4"),
                UkStartDatetime = new DateTime(2025, 1, 16, 8, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 16, 10, 40, 0),
                MaximumCapacity = 48,
                SlotLength = 10,
                Capacity = 3,
                TotalSupportedAppointmentsByService = new Dictionary<string, int>
                {
                    { "B", 0 },
                    { "C", 3 },
                    { "D", 0 },
                    { "E", 0 },
                    { "F", 0 },
                    { "G", 0 },
                },
                TotalRemainingCapacityByService = new Dictionary<string, int>
                {
                    { "B", 45 },
                    { "C", 45 },
                    { "D", 45 },
                    { "E", 45 },
                    { "F", 45 },
                    { "G", 45 },
                }
            },
            new()
            {
                Id = Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742"),
                UkStartDatetime = new DateTime(2025, 1, 16, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 16, 9, 40, 0),
                MaximumCapacity = 16,
                SlotLength = 10,
                Capacity = 4,
                TotalSupportedAppointmentsByService = new Dictionary<string, int>
                {
                    { "A", 0 },
                    { "C", 0 },
                    { "D", 0 },
                    { "U", 0 },
                    { "Z", 0 },
                    { "H", 0 },
                    { "I", 0 },
                    { "J", 0 },
                },
                TotalRemainingCapacityByService = new Dictionary<string, int>
                {
                    { "A", 16 },
                    { "C", 16 },
                    { "D", 16 },
                    { "U", 16 },
                    { "Z", 16 },
                    { "H", 16 },
                    { "I", 16 },
                    { "J", 16 },
                }
            }
        };

        var daySummaryAffected2 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 16));
        
        daySummaryAffected2.Date.Should().Be(new DateOnly(2025, 1, 16));
        daySummaryAffected2.MaximumCapacity.Should().Be(64);
        daySummaryAffected2.TotalRemainingCapacity.Should().Be(61);
        daySummaryAffected2.TotalSupportedAppointments.Should().Be(3);
        daySummaryAffected2.TotalOrphanedAppointments.Should().Be(3);
        daySummaryAffected2.TotalCancelledAppointments.Should().Be(0);
        daySummaryAffected2.SessionSummaries.Should().BeEquivalentTo(expectedSessionAvailabilitySummary2);

        //extra padded data
        var daySummaryAffected3 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 17));
        
        daySummaryAffected3.Date.Should().Be(new DateOnly(2025, 1, 17));
        daySummaryAffected3.MaximumCapacity.Should().Be(72);
        daySummaryAffected3.TotalRemainingCapacity.Should().Be(72);
        daySummaryAffected3.TotalSupportedAppointments.Should().Be(0);
        daySummaryAffected3.TotalOrphanedAppointments.Should().Be(0);
        daySummaryAffected3.TotalCancelledAppointments.Should().Be(0);
        daySummaryAffected3.SessionSummaries.Should().BeEquivalentTo([
            new SessionAvailabilitySummary
            {
                Id = Guid.Parse("ae907d84-a0e3-41d4-ae49-bed6c23d9776"),
                UkStartDatetime = new DateTime(2025, 1, 17, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 17, 12, 0, 0),
                MaximumCapacity = 72,
                SlotLength = 5,
                Capacity = 2,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "A", 0 }, { "B", 0 }, { "C", 0 }, { "D", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "A", 72 }, { "B", 72 }, { "C", 72 }, { "D", 72 } },
            }
        ]);
    }


    /// <summary>
    ///     Prove that greedy model isn't always optimal, and can lead to loss in utilisation
    ///     This test proves a scenario where just selecting the first available slot would actually have been preferable over
    ///     the alphabetical ordering
    /// </summary>
    [Fact]
    public async Task MultipleServices_LostUtilisation_2()
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

        var sessions = new List<LinkedSessionInstance>
        {
            TestSession(new DateOnly(2025, 1, 13), "09:00", "09:40", ["A", "C", "D"], capacity: 5,
                internalSessionId: Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0")),
            TestSession(new DateOnly(2025, 1, 13), "09:00", "09:40", ["A", "B", "C"], capacity: 3,
                internalSessionId: Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95")),
            TestSession(new DateOnly(2025, 1, 19), "09:00", "09:40", ["A", "C", "D"], capacity: 5,
                internalSessionId: Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4")),
            TestSession(new DateOnly(2025, 1, 19), "09:00", "09:40", ["A", "B", "C"], capacity: 3,
                internalSessionId: Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742")),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var weekSummary = await Sut.GetWeekSummary(MockSite, new DateOnly(2025, 1, 13));

        weekSummary.DaySummaries.Should().HaveCount(7);

        weekSummary.MaximumCapacity.Should().Be(64);
        weekSummary.TotalRemainingCapacity.Should().Be(58);

        //only half supported
        weekSummary.TotalSupportedAppointments.Should().Be(6);
        weekSummary.TotalOrphanedAppointments.Should().Be(6);

        var expectedSessionAvailabilitySummary1 = new List<SessionAvailabilitySummary>
        {
            new()
            {
                Id = Guid.Parse("fcff90d1-fe20-477e-af02-dac209dd86c0"),
                UkStartDatetime = new DateTime(2025, 1, 13, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 13, 9, 40, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "A", 0 }, { "C", 0 }, { "D", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "A", 20 }, { "C", 20 }, { "D", 20 } },
            },
            new()
            {
                Id = Guid.Parse("028f5107-3972-4a35-bd35-b7476442ad95"),
                UkStartDatetime = new DateTime(2025, 1, 13, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 13, 9, 40, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 3,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "A", 0 }, { "B", 0 }, { "C", 3 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "A", 9 }, { "B", 9 }, { "C", 9 } },
            }
        };

        var daySummaryAffected1 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 13));
        
        daySummaryAffected1.Date.Should().Be(new DateOnly(2025, 1, 13));
        daySummaryAffected1.MaximumCapacity.Should().Be(32);
        daySummaryAffected1.TotalRemainingCapacity.Should().Be(29);
        daySummaryAffected1.TotalSupportedAppointments.Should().Be(3);
        daySummaryAffected1.TotalOrphanedAppointments.Should().Be(3);
        daySummaryAffected1.TotalCancelledAppointments.Should().Be(0);
        daySummaryAffected1.SessionSummaries.Should().BeEquivalentTo(expectedSessionAvailabilitySummary1);

        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 14));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 15));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 16));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 17));
        weekSummary.DaySummaries.AssertEmptySessionSummariesOnDate(new DateOnly(2025, 1, 18));

        var expectedSessionAvailabilitySummary2 = new List<SessionAvailabilitySummary>
        {
            new()
            {
                Id = Guid.Parse("2c1b5938-922d-45f0-b301-5f9156bb0de4"),
                UkStartDatetime = new DateTime(2025, 1, 19, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 19, 9, 40, 0),
                MaximumCapacity = 20,
                SlotLength = 10,
                Capacity = 5,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "A", 0 }, { "C", 0 }, { "D", 0 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "A", 20 }, { "C", 20 }, { "D", 20 } },
            },
            new()
            {
                Id = Guid.Parse("d9907d84-a0e3-41d4-ae49-bed6c23d9742"),
                UkStartDatetime = new DateTime(2025, 1, 19, 9, 0, 0),
                UkEndDatetime = new DateTime(2025, 1, 19, 9, 40, 0),
                MaximumCapacity = 12,
                SlotLength = 10,
                Capacity = 3,
                TotalSupportedAppointmentsByService = new Dictionary<string, int> { { "A", 0 }, { "B", 0 }, { "C", 3 } },
                TotalRemainingCapacityByService = new Dictionary<string, int> { { "A", 9 }, { "B", 9 }, { "C", 9 } },
            }
        };

        var daySummaryAffected2 = weekSummary.DaySummaries.Single(x => x.Date == new DateOnly(2025, 1, 19));
        
        daySummaryAffected2.Date.Should().Be(new DateOnly(2025, 1, 19));
        daySummaryAffected2.MaximumCapacity.Should().Be(32);
        daySummaryAffected2.TotalRemainingCapacity.Should().Be(29);
        daySummaryAffected2.TotalSupportedAppointments.Should().Be(3);
        daySummaryAffected2.TotalOrphanedAppointments.Should().Be(3);
        daySummaryAffected2.TotalCancelledAppointments.Should().Be(0);
        daySummaryAffected2.SessionSummaries.Should().BeEquivalentTo(expectedSessionAvailabilitySummary2);
    }
}

public static class Assertions
{
    public static void AssertEmptySessionSummariesOnDate(this IEnumerable<DayAvailabilitySummary> daySummaries,
        DateOnly date)
    {
        daySummaries.Single(x => x.Date == date).Should().BeEquivalentTo(new DayAvailabilitySummary(date, []));
    }
}
