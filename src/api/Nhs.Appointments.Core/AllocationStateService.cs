namespace Nhs.Appointments.Core;

public class AllocationStateService(
    IAvailabilityStore availabilityStore,
    IBookingQueryService bookingQueryService) : IAllocationStateService
{
    public async Task<AllocationState> Build(string site, DateTime from, DateTime to, string service, bool processRecalculations = true)
    {
        var availabilityState = new AllocationState();

        var orderedLiveBookings = await bookingQueryService.GetOrderedLiveBookings(site, from, to, service);
        var slots = await GetSlots(site, DateOnly.FromDateTime(from), DateOnly.FromDateTime(to), service);

        foreach (var booking in orderedLiveBookings)
        {
            var targetSlot = ChooseHighestPrioritySlot(slots, booking);
            var bookingIsSupportedByAvailability = targetSlot is not null;

            if (bookingIsSupportedByAvailability)
            {
                if (processRecalculations && booking.AvailabilityStatus is not AvailabilityStatus.Supported)
                {
                    availabilityState.Recalculations.Add(new BookingAvailabilityUpdate(booking,
                        AvailabilityUpdateAction.SetToSupported));
                }

                targetSlot.Capacity--;
                availabilityState.Bookings.Add(booking);
                continue;
            }

            if (processRecalculations && booking.AvailabilityStatus is AvailabilityStatus.Supported &&
                booking.Status is not AppointmentStatus.Provisional)
            {
                availabilityState.Recalculations.Add(new BookingAvailabilityUpdate(booking,
                    AvailabilityUpdateAction.SetToOrphaned));
            }

            if (processRecalculations && booking.Status is AppointmentStatus.Provisional)
            {
                availabilityState.Recalculations.Add(new BookingAvailabilityUpdate(booking,
                    AvailabilityUpdateAction.ProvisionalToDelete));
            }
        }

        availabilityState.AvailableSlots = slots.Where(s => s.Capacity > 0).ToList();

        return availabilityState;
    }

    private async Task<List<SessionInstance>> GetSlots(string site, DateOnly from, DateOnly to, string service)
    {
        var sessionsOnThatDay =
            (await availabilityStore.GetSessions(site, from, to, service))
            .ToList();

        var slots = sessionsOnThatDay
            .SelectMany(session => session.ToSlots())
            .ToList();

        return slots;
    }

    /// <summary>
    /// 'Greedy' solution for assigning bookings to slots for multiple services in a deterministic way
    /// </summary>
    /// <param name="slots"></param>
    /// <param name="booking"></param>
    /// <returns></returns>
    private SessionInstance ChooseHighestPrioritySlot(List<SessionInstance> slots, Booking booking) =>
        slots.Where(sl => sl.Capacity > 0
                          && sl.From == booking.From
                          && (int)sl.Duration.TotalMinutes == booking.Duration
                          && sl.Services.Contains(booking.Service))
            .OrderBy(slot => slot.Services.Length)
            .ThenBy(slot => string.Join(string.Empty, slot.Services.Order()))
            .FirstOrDefault();
}
