using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.ApiClient
{
    public interface IBookingsApiClient
    {
        Task CancelBooking(string bookingReference);
        Task<MakeBookingResponse> MakeBooking(string site, DateTime from, string service, string sessionHolder, AttendeeDetails attendeeDetails, IEnumerable<ContactItem> contactDetails);
        Task<QueryAvailabilityResponse> QueryAvailability(string[] sites, string service, DateOnly from, DateOnly until, QueryType queryType);
        Task<IEnumerable<Booking>> QueryBookingByNhsNumber(string nhsNumber);
        Task<Booking> QueryBookingByReference(string bookingReference);
        Task<IEnumerable<Booking>> QueryBookings(string site, DateTime from, DateTime to);
        Task<SetBookingStatusResponse> SetBookingStatus(string bookingReference, string status);
    }
}
