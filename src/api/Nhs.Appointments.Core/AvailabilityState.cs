namespace Nhs.Appointments.Core;

public class AvailabilityState
{
    public List<SessionInstance> AvailableSlots = [];
    public List<AvailabilityUpdate> Recalculations = [];
    public List<Booking> Bookings = [];
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
