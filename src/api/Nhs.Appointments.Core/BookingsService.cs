using Azure.Core;
using Nhs.Appointments.Core.Concurrency;

namespace Nhs.Appointments.Core;

public interface IBookingsService
{
    Task<IEnumerable<Booking>> GetBookings(string site, DateTime from, DateTime to);
    Task<Booking> GetBookingByReference(string bookingReference);
    Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber);
    Task<(bool Success, string Reference)> MakeBooking(Booking booking);
    Task CancelBooking(string site, string bookingReference);
    Task<bool> SetBookingStatus(string bookingReference, string status);
}    

public class BookingsService : IBookingsService
{
    private readonly IAvailabilityCalculator _availabilityCalculator;
    private readonly IReferenceNumberProvider _referenceNumberProvider;
    private readonly IBookingsDocumentStore _bookingDocumentStore;
    private readonly ISiteLeaseManager _siteLeaseManager;
    
    public BookingsService(
        IBookingsDocumentStore bookingDocumentStore, 
        IReferenceNumberProvider referenceNumberProvider,
        ISiteLeaseManager siteLeaseManager, 
        IAvailabilityCalculator availabilityCalculator) 
    {
        _bookingDocumentStore = bookingDocumentStore;
        _referenceNumberProvider = referenceNumberProvider;
        _availabilityCalculator = availabilityCalculator;
        _siteLeaseManager = siteLeaseManager;
    }

    public Task<IEnumerable<Booking>> GetBookings(string site, DateTime from, DateTime to)
    {
        return _bookingDocumentStore.GetInDateRangeAsync(site, from, to);
    }
    
    public Task<Booking> GetBookingByReference(string bookingReference)
    {
        return _bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);
    }
    
    public Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber)
    {
        return _bookingDocumentStore.GetByNhsNumberAsync(nhsNumber);
    }

    public async Task<(bool Success, string Reference)> MakeBooking(Booking booking)
    {            
        using (var leaseContent = _siteLeaseManager.Acquire(booking.Site))
        {                
            var blocks = await _availabilityCalculator.CalculateAvailability(booking.Site, booking.Service, DateOnly.FromDateTime(booking.From.Date), DateOnly.FromDateTime(booking.From.Date.AddDays(1)));
            var canBook = blocks.Any(bl => bl.Contains(booking.TimePeriod));

            if (canBook)
            {
                booking.Reference = await _referenceNumberProvider.GetReferenceNumber(booking.Site);
                await _bookingDocumentStore.InsertAsync(booking);             
                return (true, booking.Reference);
            }

            return (false, string.Empty);
        }            
    }
    
    public Task CancelBooking(string site, string bookingReference)
    {
        return _bookingDocumentStore
            .BeginUpdate(site, bookingReference)
            .UpdateProperty(b => b.Outcome, "Cancelled")                
            .ApplyAsync();
    }

    public Task<bool> SetBookingStatus(string bookingReference, string status)
    {
        return _bookingDocumentStore.UpdateStatus(bookingReference, status);
    }
}