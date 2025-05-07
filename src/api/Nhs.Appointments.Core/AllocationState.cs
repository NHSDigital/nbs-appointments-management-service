namespace Nhs.Appointments.Core;

public class AllocationState
{
    public List<SessionInstance> AvailableSlots = [];
    public List<Booking> Bookings = [];
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
