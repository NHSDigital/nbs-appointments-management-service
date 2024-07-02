using Microsoft.Extensions.Logging;
using Nhs.Appointments.ApiClient.Models;

namespace Nhs.Appointments.ApiClient.Impl
{
    public class BookingsApiClient : ApiClientBase, IBookingsApiClient
    {
        public BookingsApiClient(Func<HttpClient> httpClientFactory, ILogger logger) : base(httpClientFactory, logger)
        {
        }

        public async Task<CancelBookingResponse> CancelBooking(string bookingReference, string site)
        {
            var request = new CancelBookingRequest(bookingReference, site);
            var response = await Post<CancelBookingRequest, CancelBookingResponse>(request, "api/booking/cancel");
            return response;
        }

        public async Task<MakeBookingResponse> MakeBooking(string site, DateTime from, string service, string sessionHolder, AttendeeDetails attendeeDetails)
        {
            var request = new MakeBookingRequest(site, from.ToString("yyyy-MM-dd HH:mm"), service, sessionHolder, attendeeDetails);
            var response = await Post<MakeBookingRequest, MakeBookingResponse>(request, "api/booking");
            return response;
        }

        public async Task<QueryAvailabilityResponse> QueryAvailability(string[] sites, string service, DateTime from, DateTime until, QueryType queryType)
        {
            var request = new QueryAvailabilityRequest(sites, service, from.ToString("yyyy-MM-dd"), until.ToString("yyyy-MM-dd"), queryType);
            var response = await Post<QueryAvailabilityRequest, QueryAvailabilityResponse>(request, "api/availability/query");
            return response;
        }

        public async Task<IEnumerable<Booking>> QueryBookingByNhsNumber(string nhsNumber)
        {
            var response = await Get<IEnumerable<Booking>>($"api/availability/query?nhsNumber={nhsNumber}");
            return response;
        }

        public async Task<Booking> QueryBookingByReference(string bookingReference)
        {
            var response = await Get<Booking>($"api/booking/{bookingReference}");
            return response;
        }

        public async Task<IEnumerable<Booking>> QueryBookings(string site, DateTime from, DateTime to)
        {
            var request = new QueryBookingsRequest(from, to, site);
            var response = await Post<QueryBookingsRequest, IEnumerable<Booking>>(request, "api/get-bookings");
            return response;
        }

        public async Task<SetBookingStatusResponse> SetBookingStatus(string bookingReference, string status)
        {
            var request = new SetBookingStatusRequest(bookingReference, status);
            var response = await Post<SetBookingStatusRequest, SetBookingStatusResponse>(request, "api/booking/set-status");
            return response;
        }
    }
}
