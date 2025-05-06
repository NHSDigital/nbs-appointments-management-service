namespace Nhs.Appointments.Core;

public class AllocationStateService(
    IAvailabilityQueryService availabilityQueryService,
    IBookingQueryService bookingQueryService) : IAllocationStateService
{
    public async Task<AllocationState> Build(string site, DateTime from, DateTime to, bool processRecalculations = true)
    {
        var allocationState = new AllocationState();

        var orderedLiveBookings = await bookingQueryService.GetOrderedLiveBookings(site, from, to);
        var slots = await availabilityQueryService.GetSlots(site, DateOnly.FromDateTime(from), DateOnly.FromDateTime(to));

        foreach (var booking in orderedLiveBookings)
        {
            var targetSlot = ChooseHighestPrioritySlot(slots, booking);
            var bookingIsSupportedByAvailability = targetSlot is not null;

            if (bookingIsSupportedByAvailability)
            {
                if (processRecalculations && booking.AvailabilityStatus is not AvailabilityStatus.Supported)
                {
                    allocationState.Recalculations.Add(new BookingAvailabilityUpdate(booking,
                        AvailabilityUpdateAction.SetToSupported));
                }

                targetSlot.Capacity--;
                allocationState.Bookings.Add(booking);
                continue;
            }

            if (processRecalculations && booking.AvailabilityStatus is AvailabilityStatus.Supported &&
                booking.Status is not AppointmentStatus.Provisional)
            {
                allocationState.Recalculations.Add(new BookingAvailabilityUpdate(booking,
                    AvailabilityUpdateAction.SetToOrphaned));
            }

            if (processRecalculations && booking.Status is AppointmentStatus.Provisional)
            {
                allocationState.Recalculations.Add(new BookingAvailabilityUpdate(booking,
                    AvailabilityUpdateAction.ProvisionalToDelete));
            }
        }

        allocationState.AvailableSlots = slots.Where(s => s.Capacity > 0).ToList();

        return allocationState;
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
