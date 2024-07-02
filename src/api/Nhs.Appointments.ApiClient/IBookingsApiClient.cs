using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.ApiClient
{
    public interface IBookingsApiClient
    {
        Task<CancelBookingResponse> CancelBooking(string bookingReference, string site);
        Task<MakeBookingResponse> MakeBooking(string site, DateTime from, string service, string sessionHolder, AttendeeDetails attendeeDetails);
        Task<QueryAvailabilityResponse> QueryAvailability(string[] sites, string service, DateTime from, DateTime until, QueryType queryType);
        Task<IEnumerable<Booking>> QueryBookingByNhsNumber(string nhsNumber);
        Task<Booking> QueryBookingByReference(string bookingReference);
        Task<IEnumerable<Booking>> QueryBookings(string site, DateTime from, DateTime to);
        Task<SetBookingStatusResponse> SetBookingStatus(string bookingReference, string status);
    }
}
