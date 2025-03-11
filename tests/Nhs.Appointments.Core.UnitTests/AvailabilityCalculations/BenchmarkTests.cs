using System.Diagnostics;

namespace Nhs.Appointments.Core.UnitTests.AvailabilityCalculations;

public class BenchmarkTests : AvailabilityCalculationsBase
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BenchmarkTest1(bool useV2)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        //1k random bookings
        var alphabetServices = new List<string>
        {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z"
        };

        var bookings = new List<Booking>();

        var rnd = new Random();

        //generate 10k bookings
        for (var i = 0; i < 1000; i++)
        {
            var randomAlphabetLetter = rnd.Next(0, 25);
            bookings.Add(TestBooking($"{i}", alphabetServices[randomAlphabetLetter], avStatus: "Orphaned",
                creationOrder: i));
        }

        var sessions = new List<SessionInstance>();

        //generate 100 sessions
        for (var j = 0; j < 100; j++)
        {
            //random amount of services
            var serviceArrayLength = rnd.Next(2, 10);

            var services = new List<string>();

            for (var k = 0; k < serviceArrayLength; k++)
            {
                var randomAlphabetLetter = rnd.Next(0, 25);
                services.Add(alphabetServices[randomAlphabetLetter]);
            }

            sessions.Add(TestSession("09:00", "10:00", services.Distinct().ToArray(), capacity: 1));
        }

        SetupAvailabilityAndBookings(bookings, sessions);

        AvailabilityState resultingAvailabilityState;

        if (useV2)
        {
            resultingAvailabilityState =
                await _sut.GetAvailabilityStateV2(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));
        }
        else
        {
            resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));
        }

        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }
}
