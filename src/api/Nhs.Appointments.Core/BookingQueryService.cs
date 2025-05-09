namespace Nhs.Appointments.Core;

public interface IBookingQueryService
{
    Task<Booking> GetBookingByReference(string bookingReference);
    Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber);

    Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to);
    Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site);
    Task<List<Booking>> GetOrderedLiveBookings(string site, DateTime from, DateTime to);
}

public class BookingQueryService(
    IBookingsDocumentStore bookingDocumentStore,
    TimeProvider time) : IBookingQueryService
{
    private readonly AppointmentStatus[] _liveStatuses = [AppointmentStatus.Booked, AppointmentStatus.Provisional];

    public Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to)
    {
        return bookingDocumentStore.GetCrossSiteAsync(from, to, AppointmentStatus.Booked);
    }

    public Task<Booking> GetBookingByReference(string bookingReference)
    {
        return bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);
    }

    public Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber)
    {
        return bookingDocumentStore.GetByNhsNumberAsync(nhsNumber);
    }

    public async Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site)
    {
        var bookings = await bookingDocumentStore.GetInDateRangeAsync(from, to, site);
        return bookings
            .OrderBy(b => b.From)
            .ThenBy(b => b.AttendeeDetails.LastName);
    }

    public async Task<List<Booking>> GetOrderedLiveBookings(string site, DateTime from, DateTime to)
    {
        var bookings = (await GetBookings(from, to, site))
            .Where(b => _liveStatuses.Contains(b.Status))
            .Where(b => !IsExpiredProvisional(b))
            .OrderBy(b => b.Created)
            .ToList();

        return bookings;
    }

    private bool IsExpiredProvisional(Booking b) =>
        b.Status == AppointmentStatus.Provisional && b.Created < time.GetUtcNow().AddMinutes(-5);
}
