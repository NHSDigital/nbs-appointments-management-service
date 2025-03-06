namespace Nhs.Appointments.Core;

public class AvailabilityState
{
    public List<SessionInstance> AvailableSlots = default;
    public List<AvailabilityUpdate> Recalculations = default;
    public List<Booking> Bookings = default;
}

public class AvailabilityUpdate(Booking booking, AvailabilityUpdateAction action)
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
