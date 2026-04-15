
namespace Nhs.Appointments.Core.Bookings;

public class BookingDayRange(DateOnly date)
{
    public DateTime Start { get; } = new(date, new TimeOnly(0, 0), DateTimeKind.Unspecified);

    public DateTime End { get; } = new(date, new TimeOnly(23, 59), DateTimeKind.Unspecified);
}
