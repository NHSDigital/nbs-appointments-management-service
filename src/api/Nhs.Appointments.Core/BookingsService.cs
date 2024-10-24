using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core;

public interface IBookingsService
{
    Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site);
    Task<Booking> GetBookingByReference(string bookingReference);
    Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber);
    Task<(bool Success, string Reference, bool Provisional, Uri? ConfirmationEndpoint)> MakeBooking(Booking booking);
    Task CancelBooking(string site, string bookingReference);
    Task<bool> SetBookingStatus(string bookingReference, string status);
    Task SendBookingReminders();
    Task<bool> ConfirmProvisionalBooking(string bookingReference);
    Task RemoveUnconfirmedProvisionalBookings();
}    

public class BookingsService : IBookingsService
{
    private readonly IAvailabilityCalculator _availabilityCalculator;
    private readonly IReferenceNumberProvider _referenceNumberProvider;
    private readonly IBookingsDocumentStore _bookingDocumentStore;
    private readonly ISiteLeaseManager _siteLeaseManager;
    private readonly IMessageBus _bus;
    private readonly TimeProvider _time;

    public BookingsService(
        IBookingsDocumentStore bookingDocumentStore, 
        IReferenceNumberProvider referenceNumberProvider,
        ISiteLeaseManager siteLeaseManager, 
        IAvailabilityCalculator availabilityCalculator,
        IMessageBus bus,
        TimeProvider time) 
    {
        _bookingDocumentStore = bookingDocumentStore;
        _referenceNumberProvider = referenceNumberProvider;
        _availabilityCalculator = availabilityCalculator;
        _siteLeaseManager = siteLeaseManager;
        _bus = bus;
        _time = time;
    }

    public Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site)
    {
        return _bookingDocumentStore.GetInDateRangeAsync(from, to, site);
    }

    protected Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to)
    {
        return _bookingDocumentStore.GetCrossSiteAsync(from, to, false);
    }

    public Task<Booking> GetBookingByReference(string bookingReference)
    {
        return _bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);
    }
    
    public Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber)
    {
        return _bookingDocumentStore.GetByNhsNumberAsync(nhsNumber);
    }

    public async Task<(bool Success, string Reference, bool Provisional, Uri? ConfirmationEndpoint)> MakeBooking(Booking booking)
    {            
        using (var leaseContent = _siteLeaseManager.Acquire(booking.Site))
        {                
            var slots = await _availabilityCalculator.CalculateAvailability(booking.Site, booking.Service, DateOnly.FromDateTime(booking.From.Date), DateOnly.FromDateTime(booking.From.Date.AddDays(1)));
            var canBook = slots.Any(sl => sl.From == booking.From && sl.Duration.TotalMinutes == booking.Duration);

            if (canBook)
            {
                booking.Created = _time.GetLocalNow().DateTime;
                booking.Reference = await _referenceNumberProvider.GetReferenceNumber(booking.Site);
                booking.ReminderSent = false;
                await _bookingDocumentStore.InsertAsync(booking);

                if (!booking.Provisional)
                {
                    var bookingMadeEvent = BuildBookingMadeEvent(booking);
                    await _bus.Send(bookingMadeEvent);
                }

                return (true, booking.Reference, booking.Provisional, GetConfirmationEndpointForProvisionalBooking(booking));
            }

            return (false, string.Empty, booking.Provisional, null);
        }            
    }

    private Uri? GetConfirmationEndpointForProvisionalBooking(Booking booking)
    {
        // TODO: implement this endpoint and lock in the correct URI here
        return booking.Provisional ? new Uri($"https://TODO/booking/{booking.Reference}/confirm") : null;
    }

    public Task CancelBooking(string site, string bookingReference)
    {
        return _bookingDocumentStore
            .BeginUpdate(site, bookingReference)
            .UpdateProperty(b => b.Outcome, "Cancelled")                
            .ApplyAsync();
    }

    public Task<bool> ConfirmProvisionalBooking(string bookingReference)
    {
        return _bookingDocumentStore.ConfirmProvisional(bookingReference);
    }

    public Task<bool> SetBookingStatus(string bookingReference, string status)
    {
        return _bookingDocumentStore.UpdateStatus(bookingReference, status);
    }

    public async Task SendBookingReminders()
    {
        var bookings = await GetBookings(_time.GetLocalNow().DateTime, _time.GetLocalNow().AddDays(3).DateTime);
        foreach (var booking in bookings.Where(b => !b.ReminderSent))
        {
            var reminder = BuildReminderEvent(booking);
            await _bus.Send(reminder);
            booking.ReminderSent = true;
            await _bookingDocumentStore.SetReminderSent(booking.Reference, booking.Site);
        }
    }

    private static BookingMade BuildBookingMadeEvent(Booking booking)
    {
        if(booking.ContactDetails == null)
        {
            throw new ArgumentException("The booking must include contact details");
        }

        return new BookingMade
        {
            FirstName = booking.AttendeeDetails.FirstName,
            From = booking.From,
            LastName = booking.AttendeeDetails.LastName,
            Reference = booking.Reference,
            Service = booking.Service,
            Site = booking.Site,
            ContactDetails = booking.ContactDetails.Select(c => new Messaging.Events.ContactItem { Type = c.Type, Value = c.Value}).ToArray()
        };
    }

    private static BookingReminder BuildReminderEvent(Booking booking)
    {
        if (booking.ContactDetails == null)
        {
            throw new ArgumentException("The booking must include contact details");
        }

        return new BookingReminder
        {
            FirstName = booking.AttendeeDetails.FirstName,
            From = booking.From,
            LastName = booking.AttendeeDetails.LastName,
            Reference = booking.Reference,
            Service = booking.Service,
            Site = booking.Site,
            ContactDetails = booking.ContactDetails.Select(c => new Messaging.Events.ContactItem { Type = c.Type, Value = c.Value }).ToArray()
        };
    }

    public Task RemoveUnconfirmedProvisionalBookings()
    {
        return _bookingDocumentStore.RemoveUnconfirmedProvisionalBookings();
    }
}