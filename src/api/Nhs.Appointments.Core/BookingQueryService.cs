namespace Nhs.Appointments.Core;

public interface IBookingQueryService
{
    Task<Booking> GetBookingByReference(string bookingReference);
    Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber);

    Task<IEnumerable<Booking>> GetBookedBookingsAcrossAllSites(DateTime from, DateTime to);
    Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site);
    Task<IEnumerable<Booking>> GetOrderedBookings(string site, DateTime from, DateTime to, IEnumerable<AppointmentStatus> statuses);
}

public class BookingQueryService(
    IBookingsDocumentStore bookingDocumentStore,
    TimeProvider time) : IBookingQueryService
{
    public Task<IEnumerable<Booking>> GetBookedBookingsAcrossAllSites(DateTime from, DateTime to)
    {
        return bookingDocumentStore.GetCrossSiteAsync(from, to, AppointmentStatus.Booked);
    }

    public async Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site)
    {
        var bookings = await bookingDocumentStore.GetInDateRangeAsync(from, to, site);
        return bookings
            .OrderBy(b => b.From)
            .ThenBy(b => b.AttendeeDetails.LastName);
    }

    public Task<Booking> GetBookingByReference(string bookingReference)
    {
        return bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);
    }

    public Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber)
    {
        return bookingDocumentStore.GetByNhsNumberAsync(nhsNumber);
    }

    public async Task<IEnumerable<Booking>> GetOrderedBookings(string site, DateTime from, DateTime to, IEnumerable<AppointmentStatus> statuses)
    {
        var bookings = (await GetBookings(from, to, site))
            .Where(b => statuses.Contains(b.Status))
            .Where(b => !IsExpiredProvisional(b))
            .OrderBy(b => b.Created);

        return bookings;
    }

    private bool IsExpiredProvisional(Booking b) =>
        b.Status == AppointmentStatus.Provisional && b.Created < time.GetUtcNow().AddMinutes(-5);
}
