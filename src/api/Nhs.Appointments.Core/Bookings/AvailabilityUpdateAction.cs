namespace Nhs.Appointments.Core.Bookings;

public enum AvailabilityUpdateAction
{
    Default,
    ProvisionalToDelete,
    SetToSupported,
    SetToOrphaned,
    SetToCancelled,
}
