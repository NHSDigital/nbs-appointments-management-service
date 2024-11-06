using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core;

public interface IBookingsService
{
    Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site);
    Task<Booking> GetBookingByReference(string bookingReference);
    Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber);
    Task<(bool Success, string Reference, bool Provisional)> MakeBooking(Booking booking);
    Task<BookingCancellationResult> CancelBooking(string site, string bookingReference);
    Task<bool> SetBookingStatus(string bookingReference, string status);
    Task SendBookingReminders();
    Task<BookingConfirmationResult> ConfirmProvisionalBooking(string bookingReference, IEnumerable<ContactItem> contactDetails);
    Task RemoveUnconfirmedProvisionalBookings();
}    

public class BookingsService(
        IBookingsDocumentStore bookingDocumentStore,
        IReferenceNumberProvider referenceNumberProvider,
        ISiteLeaseManager siteLeaseManager,
        IAvailabilityCalculator availabilityCalculator,
        IMessageBus bus,
        TimeProvider time) : IBookingsService
{ 
    public Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site)
    {
        return bookingDocumentStore.GetInDateRangeAsync(from, to, site);
    }

    protected Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to)
    {
        return bookingDocumentStore.GetCrossSiteAsync(from, to, false);
    }

    public Task<Booking> GetBookingByReference(string bookingReference)
    {
        return bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);
    }
    
    public Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber)
    {
        return bookingDocumentStore.GetByNhsNumberAsync(nhsNumber);
    }

    public async Task<(bool Success, string Reference, bool Provisional)> MakeBooking(Booking booking)
    {            
        using (var leaseContent = siteLeaseManager.Acquire(booking.Site))
        {                
            var slots = await availabilityCalculator.CalculateAvailability(booking.Site, booking.Service, DateOnly.FromDateTime(booking.From.Date), DateOnly.FromDateTime(booking.From.Date.AddDays(1)));
            var canBook = slots.Any(sl => sl.From == booking.From && sl.Duration.TotalMinutes == booking.Duration);

            if (canBook)
            {
                booking.Created = time.GetUtcNow().DateTime;
                booking.Reference = await referenceNumberProvider.GetReferenceNumber(booking.Site);
                booking.ReminderSent = false;
                await bookingDocumentStore.InsertAsync(booking);

                if (!booking.Provisional)
                {
                    var bookingMadeEvent = BuildBookingMadeEvent(booking);
                    await bus.Send(bookingMadeEvent);
                }

                return (true, booking.Reference, booking.Provisional);
            }

            return (false, string.Empty, booking.Provisional);
        }            
    }

    public async Task<BookingCancellationResult> CancelBooking(string site, string bookingReference)
    {
        var booking = await bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);

        if (booking == null)
        {
            return BookingCancellationResult.NotFound;
        }

        await bookingDocumentStore
            .BeginUpdate(site, bookingReference)
            .UpdateProperty(b => b.Outcome, "Cancelled")                
            .ApplyAsync();

        var bookingCancelledEvent = BuildBookingCancelledEvent(booking);
        await bus.Send (bookingCancelledEvent);

        return BookingCancellationResult.Success;
    }

    public Task<BookingConfirmationResult> ConfirmProvisionalBooking(string bookingReference, IEnumerable<ContactItem> contactDetails)
    {
        return bookingDocumentStore.ConfirmProvisional(bookingReference, contactDetails);
    }

    public Task<bool> SetBookingStatus(string bookingReference, string status)
    {
        return bookingDocumentStore.UpdateStatus(bookingReference, status);
    }

    public async Task SendBookingReminders()
    {
        var bookings = await GetBookings(time.GetLocalNow().DateTime, time.GetLocalNow().AddDays(3).DateTime);
        foreach (var booking in bookings.Where(b => !b.ReminderSent))
        {
            var reminder = BuildReminderEvent(booking);
            await bus.Send(reminder);
            booking.ReminderSent = true;
            await bookingDocumentStore.SetReminderSent(booking.Reference, booking.Site);
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

    private static BookingCancelled BuildBookingCancelledEvent(Booking booking)
    {
        return new BookingCancelled
        {
            FirstName = booking.AttendeeDetails?.FirstName,
            From = booking.From,
            LastName = booking.AttendeeDetails?.LastName,
            Reference = booking.Reference,
            Service = booking.Service,
            Site = booking.Site,
            ContactDetails = booking.ContactDetails?.Select(c => new Messaging.Events.ContactItem { Type = c.Type, Value = c.Value }).ToArray()
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
        return bookingDocumentStore.RemoveUnconfirmedProvisionalBookings();
    }
}