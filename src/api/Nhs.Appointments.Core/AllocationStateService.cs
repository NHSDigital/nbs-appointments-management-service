namespace Nhs.Appointments.Core;

public class AllocationStateService(
    IAvailabilityQueryService availabilityQueryService,
    IBookingQueryService bookingQueryService) : IAllocationStateService
{
    public async Task<AllocationState> BuildAllocation(string site, DateTime from, DateTime to)
    {
        var bookingsTask = GetBookings(site, from, to);
        var slotsTask = GetSlots(site, from, to);
        await Task.WhenAll(bookingsTask, slotsTask);

        return BuildAllocation(bookingsTask.Result.ToList(), slotsTask.Result.ToList());
    }

    public async Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from,
        DateTime to)
    {
        var recalculations = new List<BookingAvailabilityUpdate>();

        var bookingsTask = GetBookings(site, from, to);
        var slotsTask = GetSlots(site, from, to);
        await Task.WhenAll(bookingsTask, slotsTask);

        var bookings = bookingsTask.Result.ToList();
        var state = BuildAllocation(bookings, slotsTask.Result.ToList());

        var supportedReferences = state.SupportedBookings.Select(x => x.Reference).ToList();

        recalculations.AddRange(bookings
            .Where(booking => supportedReferences.Contains(booking.Reference) &&
                              booking.AvailabilityStatus is not AvailabilityStatus.Supported)
            .Select(booking => new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.SetToSupported)));
        recalculations.AddRange(bookings
            .Where(booking => !supportedReferences.Contains(booking.Reference) &&
                              booking.AvailabilityStatus is AvailabilityStatus.Supported &&
                              booking.Status is not AppointmentStatus.Provisional)
            .Select(booking => new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.SetToOrphaned)));
        recalculations.AddRange(bookings
            .Where(booking => !supportedReferences.Contains(booking.Reference) &&
                              booking.Status is AppointmentStatus.Provisional)
            .Select(booking => new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.ProvisionalToDelete)));

        return recalculations;
    }

    private AllocationState BuildAllocation(List<Booking> orderedLiveBookings, List<SessionInstance> slots)
    {
        var allocationState = new AllocationState();

        foreach (var booking in orderedLiveBookings)
        {
            var targetSlot = ChooseHighestPrioritySlot(slots, booking);
            var bookingIsSupportedByAvailability = targetSlot is not null;

            if (bookingIsSupportedByAvailability)
            {
                targetSlot.Capacity--;
                allocationState.SupportedBookings.Add(booking);
            }
        }

        allocationState.AvailableSlots = slots.Where(s => s.Capacity > 0).ToList();

        return allocationState;
    }

    /// <summary>
    ///     'Greedy' solution for assigning bookings to slots for multiple services in a deterministic way
    /// </summary>
    /// <param name="slots"></param>
    /// <param name="booking"></param>
    /// <returns></returns>
    private SessionInstance ChooseHighestPrioritySlot(IEnumerable<SessionInstance> slots, Booking booking) =>
        slots.Where(sl => sl.Capacity > 0
                          && sl.From == booking.From
                          && (int)sl.Duration.TotalMinutes == booking.Duration
                          && sl.Services.Contains(booking.Service))
            .OrderBy(slot => slot.Services.Length)
            .ThenBy(slot => string.Join(string.Empty, slot.Services.Order()))
            .FirstOrDefault();

    private async Task<IEnumerable<Booking>> GetBookings(string site, DateTime from, DateTime to) =>
        await bookingQueryService.GetOrderedLiveBookings(site, from, to);

    private async Task<IEnumerable<SessionInstance>> GetSlots(string site, DateTime from, DateTime to) =>
        await availabilityQueryService.GetSlots(site, DateOnly.FromDateTime(from), DateOnly.FromDateTime(to));
}
