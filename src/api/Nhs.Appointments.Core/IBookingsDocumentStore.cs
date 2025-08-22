using System.Linq.Expressions;

namespace Nhs.Appointments.Core;

public interface IBookingsDocumentStore 
{
    Task InsertAsync(Booking booking);
    Task<IEnumerable<Booking>> GetInDateRangeAsync(DateTime from, DateTime to, string site);
    Task<IEnumerable<Booking>> GetCrossSiteAsync(DateTime from, DateTime to, params AppointmentStatus[] statuses);
    Task<Booking> GetByReferenceOrDefaultAsync(string bookingReference);
    Task<IEnumerable<Booking>> GetByNhsNumberAsync(string nhsNumber);
    Task<bool> UpdateStatus(string bookingReference, AppointmentStatus status, AvailabilityStatus availabilityStatus, CancellationReason? cancellationReason = null);
    IDocumentUpdate<Booking> BeginUpdate(string site, string reference);
    Task SetReminderSent(string bookingReference, string site);
    Task<BookingConfirmationResult> ConfirmProvisionals(string[] bookingReferences, IEnumerable<ContactItem> contactDetails);
    Task<BookingConfirmationResult> ConfirmProvisional(string bookingReference, IEnumerable<ContactItem> contactDetails, string bookingToReschedule, CancellationReason? cancellationReason = null);
    Task<IEnumerable<string>> RemoveUnconfirmedProvisionalBookings();
    Task DeleteBooking(string reference, string site);
    Task<bool> UpdateAvailabilityStatus(string bookingReference, AvailabilityStatus status);
    Task<(int cancelledBookingsCount, int bookingsWithoutContactDetailsCount)> CancelAllBookingsInDay(string site, DateOnly date);
}

public interface IRolesStore
{
    Task<IEnumerable<Role>> GetRoles();
}

public interface INotificationConfigurationStore
{
    Task<IEnumerable<NotificationConfiguration>> GetNotificationConfiguration();    
}

public interface IDocumentUpdate<TModel>
{
    IDocumentUpdate<TModel> UpdateProperty<TProp>(Expression<Func<TModel, TProp>> prop, TProp val);
    Task ApplyAsync();
}

public enum BookingConfirmationResult
{
    Unknown,
    Success,
    Expired,
    NotFound,
    RescheduleNotFound,
    RescheduleMismatch,
    StatusMismatch,
    GroupBookingInvalid
}

public enum BookingCancellationResult
{
    Success,
    NotFound
}

public enum BookingCancellationReason
{
    Citizen,
    Site
}
