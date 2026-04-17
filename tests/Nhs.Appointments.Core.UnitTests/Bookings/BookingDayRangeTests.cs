using Nhs.Appointments.Core.Bookings;

namespace Nhs.Appointments.Core.UnitTests.Bookings;

public class BookingDayRangeTests
{
    [Theory]
    [MemberData(nameof(DayValuesData))]
    public void CreateBookingDayRange_WhenDayValuesArePassed_DerivesCorrectStartAndEndValues(DateOnly day)
    {
        // Arrange.
        var expectedStart = new DateTime(day, new TimeOnly(0, 0));
        var expectedEnd = new DateTime(day, new TimeOnly(23, 59));
        var sut = new BookingDayRange(day);

        // Act - not required.

        // Assert.
        sut.Start.Should().Be(expectedStart);
        sut.End.Should().Be(expectedEnd);
    }

    public static IEnumerable<object[]> DayValuesData =>
    [
        [new DateOnly(2026, 01, 01)],
        [new DateOnly(2026, 12, 31)], 
        [new DateOnly(2024, 1, 29)],
        [new DateOnly(2027, 8, 15)] 
    ];
}
