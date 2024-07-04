using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.ApiClient.Configuration;
using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.ApiClient.Impl
{
    public class BookingsApiClient : ApiClientBase, IBookingsApiClient
    {
        public BookingsApiClient([FromKeyedServices(ContainerConfiguration.HttpClientKey)] Func<HttpClient> httpClientFactory, ILogger<BookingsApiClient> logger) : base(httpClientFactory, logger)
        {
        }

        public Task<CancelBookingResponse> CancelBooking(string bookingReference, string site) => Post<CancelBookingRequest, CancelBookingResponse>("api/booking/cancel", new CancelBookingRequest(bookingReference, site));

        public Task<MakeBookingResponse> MakeBooking(string site, DateTime from, string service, string sessionHolder, AttendeeDetails attendeeDetails) => Post<MakeBookingRequest, MakeBookingResponse>("api/booking", new MakeBookingRequest(site, from.ToString("yyyy-MM-dd HH:mm"), service, sessionHolder, attendeeDetails));

        public Task<QueryAvailabilityResponse> QueryAvailability(string[] sites, string service, DateOnly from, DateOnly until, QueryType queryType) => Post<QueryAvailabilityRequest, QueryAvailabilityResponse>("api/availability/query", new QueryAvailabilityRequest(sites, service, from.ToString("yyyy-MM-dd"), until.ToString("yyyy-MM-dd"), queryType));

        public Task<IEnumerable<Booking>> QueryBookingByNhsNumber(string nhsNumber) => Get<IEnumerable<Booking>>($"api/availability/query?nhsNumber={nhsNumber}");

        public Task<Booking> QueryBookingByReference(string bookingReference) => Get<Booking>($"api/booking/{bookingReference}");

        public Task<IEnumerable<Booking>> QueryBookings(string site, DateTime from, DateTime to) => Post<QueryBookingsRequest, IEnumerable<Booking>>("api/get-bookings", new QueryBookingsRequest(from, to, site));

        public Task<SetBookingStatusResponse> SetBookingStatus(string bookingReference, string status) => Post<SetBookingStatusRequest, SetBookingStatusResponse>("api/booking/set-status", new SetBookingStatusRequest(bookingReference, status));
    }
}
