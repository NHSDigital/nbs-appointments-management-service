using System.Diagnostics;

namespace Nhs.Appointments.Core.UnitTests.AvailabilityCalculations;

public class BenchmarkTests : AvailabilityCalculationsBase
{
    [Fact(Skip = "This was just curiosity")]
    public async Task BenchmarkTest1()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

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

        for (var i = 0; i < 5000; i++)
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

           var resultingAvailabilityState =
                await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        stopwatch.Stop();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
    }
}
