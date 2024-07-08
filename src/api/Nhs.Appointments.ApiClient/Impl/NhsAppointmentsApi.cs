namespace Nhs.Appointments.ApiClient.Impl
{
    public class NhsAppointmentsApi : INhsAppointmentsApi
    {
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly ISitesApiClient _sitesApiClient;
        private readonly ITemplatesApiClient _templatesApiClient;

        public NhsAppointmentsApi(IBookingsApiClient bookingsApiClient, ISitesApiClient sitesApiClient, ITemplatesApiClient templatesApiClient)
        {
            _bookingsApiClient = bookingsApiClient;
            _sitesApiClient = sitesApiClient;
            _templatesApiClient = templatesApiClient;
        }

        public IBookingsApiClient Bookings => _bookingsApiClient;
        public ISitesApiClient Sites => _sitesApiClient;
        public ITemplatesApiClient Templates => _templatesApiClient;
    }
}
