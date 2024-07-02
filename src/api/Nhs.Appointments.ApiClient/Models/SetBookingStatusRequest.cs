namespace Nhs.Appointments.ApiClient.Models
{
    public record SetBookingStatusRequest(string bookingReference, string status);
}
