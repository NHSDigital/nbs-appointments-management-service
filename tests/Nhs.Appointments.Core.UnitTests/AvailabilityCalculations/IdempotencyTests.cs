﻿namespace Nhs.Appointments.Core.UnitTests.AvailabilityCalculations;

// See https://app.mural.co/t/nhsdigital8118/m/nhsdigital8118/1741252759332/f2fa27aa39fa459db285e5c6c8081e9bb7446d22
public class IdempotencyTests : AvailabilityCalculationsBase
{
    [Fact]
    public async Task MultiplePassesProduceTheSameResult_BestFitModel()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Orange"),
            TestBooking("2", "Blue", creationOrder: 2),
            TestBooking("3", "Green", creationOrder: 3),
            TestBooking("4", "Orange", creationOrder: 4),
            TestBooking("5", "Orange", creationOrder: 5),
            TestBooking("6", "Green", creationOrder: 6),
            TestBooking("7", "Green", "09:10", creationOrder: 7),
            TestBooking("8", "Blue", "09:10", creationOrder: 8),
            TestBooking("9", "Orange", "09:10", creationOrder: 9),
            TestBooking("10", "Blue", "09:30", creationOrder: 10),
            TestBooking("11", "Green", "09:30", creationOrder: 11),
            TestBooking("12", "Green", "09:30", creationOrder: 12),
            TestBooking("13", "Green", "09:30", creationOrder: 13),
            TestBooking("14", "Green", "09:40", creationOrder: 14),
            TestBooking("15", "Orange", "09:40", creationOrder: 15),
            TestBooking("16", "Blue", "09:40", creationOrder: 16),
            TestBooking("17", "Green", "09:40", creationOrder: 17),
            TestBooking("18", "Orange", "09:50", creationOrder: 18),
            TestBooking("19", "Green", "09:50", creationOrder: 19),
            TestBooking("20", "Blue", "09:50", creationOrder: 20),
            TestBooking("21", "Blue", "09:50", creationOrder: 21)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue", "Orange"], capacity: 2),
            TestSession("09:00", "09:40", ["Blue", "Green"], capacity: 1),
            TestSession("09:30", "10:00", ["Orange", "Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var firstRunResult = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        firstRunResult.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "18", "19", "20");

        // Bookings 4 and 5 should not be, because they were created after 6 and 7
        firstRunResult.Recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        firstRunResult.Recalculations.Should().NotContain(r => r.Booking.Reference == "5");
        firstRunResult.Recalculations.Should().NotContain(r => r.Booking.Reference == "6");
        firstRunResult.Recalculations.Should().NotContain(r => r.Booking.Reference == "17");
        firstRunResult.Recalculations.Should().NotContain(r => r.Booking.Reference == "21");

        var runs = 10;
        while (runs > 0)
        {
            var newResult = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));
            newResult.Should().BeEquivalentTo(firstRunResult);
            runs -= 1;
        }
    }
}
