namespace Nhs.Appointments.Core.Bookings;

/// <summary>
/// Decide what to do with bookings that have changed from Supported -> Unsupported
/// </summary>
public enum NewlyUnsupportedBookingAction
{
    Orphan, //AvailabilityUpdateAction.SetToOrphaned
    Cancel //AvailabilityUpdateAction.SetToCancelled
}
