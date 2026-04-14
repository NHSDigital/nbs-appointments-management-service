using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Core.Bookings;

public class BookingAvailabilityState
{
    public readonly List<BookingAvailabilityUpdate> BookingAvailabilityUpdates = [];

    public IEnumerable<SessionInstance> AvailableSlots { get; set; } = [];

    public AvailabilitySummary Summary { get; set; }

    public AvailabilityUpdateProposal UpdateProposal { get; set; } = new AvailabilityUpdateProposal();
}
