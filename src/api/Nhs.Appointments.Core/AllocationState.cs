namespace Nhs.Appointments.Core;

public class AllocationState
{
    public readonly List<string> SupportedBookingReferences = [];

    public List<SessionInstance> AvailableSlots { get; set; } = [];
}

public class BookingAvailabilityUpdate(Booking booking, AvailabilityUpdateAction action)
{
    public Booking Booking { get; } = booking;
    public AvailabilityUpdateAction Action { get; } = action;
}

public enum AvailabilityUpdateAction
{
    Default,
    ProvisionalToDelete,
    SetToSupported,
    SetToOrphaned
}
