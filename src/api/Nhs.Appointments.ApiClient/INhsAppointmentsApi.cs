namespace Nhs.Appointments.ApiClient
{
    public interface INhsAppointmentsApi
    {
        IBookingsApiClient Bookings { get; }
        ISitesApiClient Sites { get; }
        ITemplatesApiClient Templates { get; }
    }
}
