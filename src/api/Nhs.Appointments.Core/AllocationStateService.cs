namespace Nhs.Appointments.Core;

public class AllocationStateService(
    IAvailabilityQueryService availabilityQueryService,
    IBookingQueryService bookingQueryService) : IAllocationStateService
{
    public async Task<AllocationState> BuildAllocation(string site, DateTime from, DateTime to)
    {
        var (bookings, slots) = await FetchData(site, from, to);
        return BuildAllocation(bookings, slots);
    }

    public async Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from,
        DateTime to)
    {
        var recalculations = new List<BookingAvailabilityUpdate>();

        var (bookings, slots) = await FetchData(site, from, to);
        var bookingsList = bookings.ToList();
        var state = BuildAllocation(bookingsList, slots);

        recalculations.AppendNewlySupportedBookings(state, bookingsList);
        recalculations.AppendNoLongerSupportedBookings(state, bookingsList);
        recalculations.AppendProvisionalBookingsToBeDeleted(state, bookingsList);

        return recalculations;
    }

    private async Task<(IEnumerable<Booking> bookings, IEnumerable<SessionInstance> slots)> FetchData(string site, DateTime from, DateTime to)
    {
        var bookingsTask = bookingQueryService.GetOrderedLiveBookings(site, from, to);
        var slotsTask = availabilityQueryService.GetSlots(site, DateOnly.FromDateTime(from), DateOnly.FromDateTime(to));
        await Task.WhenAll(bookingsTask, slotsTask);
        return (bookingsTask.Result, slotsTask.Result);
    }

    private static AllocationState BuildAllocation(IEnumerable<Booking> orderedLiveBookings, IEnumerable<SessionInstance> slots)
    {
        var allocationState = new AllocationState();
        var slotsList = slots.ToList();

        foreach (var booking in orderedLiveBookings)
        {
            var targetSlot = ChooseHighestPrioritySlot(slotsList, booking);
            var bookingIsSupportedByAvailability = targetSlot is not null;

            if (bookingIsSupportedByAvailability)
            {
                targetSlot.Capacity--;
                
                //TODO this still is only needed for when running a recalculation
                //waste of memory etc for when no recalculations required
                allocationState.SupportedBookingReferences.Add(booking.Reference);
            }
        }

        allocationState.AvailableSlots = slotsList.Where(s => s.Capacity > 0).ToList();

        return allocationState;
    }

    /// <summary>
    ///     'Greedy' solution for assigning bookings to slots for multiple services in a deterministic way
    /// </summary>
    /// <param name="slots"></param>
    /// <param name="booking"></param>
    /// <returns></returns>
    private static SessionInstance ChooseHighestPrioritySlot(IEnumerable<SessionInstance> slots, Booking booking) =>
        slots.Where(sl => sl.Capacity > 0
                          && sl.From == booking.From
                          && (int)sl.Duration.TotalMinutes == booking.Duration
                          && sl.Services.Contains(booking.Service))
            .OrderBy(slot => slot.Services.Length)
            .ThenBy(slot => string.Join(string.Empty, slot.Services.Order()))
            .FirstOrDefault();
}

public static class RecalculationExtensions
{
    public static void AppendNewlySupportedBookings(this List<BookingAvailabilityUpdate> recalculations, AllocationState state, IEnumerable<Booking> bookings)
    {
        recalculations.AddRange(bookings
            .Where(booking => state.SupportedBookingReferences.Contains(booking.Reference) &&
                              booking.AvailabilityStatus is not AvailabilityStatus.Supported)
            .Select(booking => new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.SetToSupported)));
    }

    public static void AppendNoLongerSupportedBookings(this List<BookingAvailabilityUpdate> recalculations, AllocationState state, IEnumerable<Booking> bookings)
    {
        recalculations.AddRange(bookings
            .Where(booking => !state.SupportedBookingReferences.Contains(booking.Reference) &&
                              booking.AvailabilityStatus is AvailabilityStatus.Supported &&
                              booking.Status is not AppointmentStatus.Provisional)
            .Select(booking => new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.SetToOrphaned)));
    }

    public static void AppendProvisionalBookingsToBeDeleted(this List<BookingAvailabilityUpdate> recalculations, AllocationState state, IEnumerable<Booking> bookings)
    {
        recalculations.AddRange(bookings
            .Where(booking => !state.SupportedBookingReferences.Contains(booking.Reference) &&
                              booking.Status is AppointmentStatus.Provisional)
            .Select(booking => new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.ProvisionalToDelete)));
    }
}
