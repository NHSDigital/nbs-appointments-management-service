namespace Nhs.Appointments.Core;

public class SessionModificationResult(
    bool updateSuccessful,
    string message,
    int? bookingsCanceled = null,
    int? bookingsCanceledWithoutDetails = null)
{
    
    public bool UpdateSuccessful { get; set; } = updateSuccessful;
    public string Message { get; set; } = message;
    public int? BookingsCanceled { get; set; } = bookingsCanceled;
    public int? BookingsCanceledWithoutDetails { get; set; } = bookingsCanceledWithoutDetails;
}
