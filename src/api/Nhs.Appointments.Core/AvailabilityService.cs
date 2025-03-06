using Nhs.Appointments.Core.Concurrency;

namespace Nhs.Appointments.Core;

public class AvailabilityService(
    IAvailabilityStore availabilityStore,
    IAvailabilityCreatedEventStore availabilityCreatedEventStore,
    IBookingsService bookingsService,
    ISiteLeaseManager siteLeaseManager) : IAvailabilityService
{
    private readonly AppointmentStatus[] _liveStatuses = [AppointmentStatus.Booked, AppointmentStatus.Provisional];

    internal async Task<List<Booking>> GetOrderedLiveBookings(string site, DateOnly day)
    {
        var dayStart = day.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = day.ToDateTime(new TimeOnly(23, 59));

        var bookings = (await bookingsService.GetBookings(dayStart, dayEnd, site))
            .Where(b => _liveStatuses.Contains(b.Status))
            .OrderBy(b => b.Created)
            .ToList();

        return bookings;
    }

    private async Task<List<SessionInstance>> GetSlots(string site, DateOnly day)
    {
        var sessionsOnThatDay =
            (await availabilityStore.GetSessions(site, day, day))
            .ToList();

        var slots = sessionsOnThatDay
            .SelectMany(session => session.ToSlots())
            .ToList();

        return slots;
    }

    public async Task ApplyAvailabilityTemplateAsync(string site, DateOnly from, DateOnly until, Template template, ApplyAvailabilityMode mode, string user)
    {
        if (string.IsNullOrEmpty(site))
            throw new ArgumentException("site must have a value");

        if (from > until)
            throw new ArgumentException("until date must be after from date");

        if (template == null)
            throw new ArgumentException("template must be provided");

        if (template.Sessions is null || template.Sessions.Length == 0)
            throw new ArgumentException("template must contain one or more sessions");

        if (template.Days is null || template.Days.Length == 0)
            throw new ArgumentException("template must specify one or more weekdays");

        var dates = GetDatesBetween(from, until, template.Days);
        foreach (var date in dates)
        {
            await availabilityStore.ApplyAvailabilityTemplate(site, date, template.Sessions, mode);

            if (TemporaryFeatureToggles.MultiServiceAvailabilityCalculations)
            {
                await RecalculateAppointmentStatuses(site, date);
            }
            else
            {
                await bookingsService.RecalculateAppointmentStatuses(site, date);
            }

        }

        await availabilityCreatedEventStore.LogTemplateCreated(site, from, until, template, user);
    }

    public async Task ApplySingleDateSessionAsync(DateOnly date, string site, Session[] sessions,
        ApplyAvailabilityMode mode, string user, Session sessionToEdit = null)
    {
        await SetAvailabilityAsync(date, site, sessions, mode, sessionToEdit);

        if (TemporaryFeatureToggles.MultiServiceAvailabilityCalculations)
        {
            await RecalculateAppointmentStatuses(site, date);
        }
        else
        {
            await bookingsService.RecalculateAppointmentStatuses(site, date);
        }

        await availabilityCreatedEventStore.LogSingleDateSessionCreated(site, date, sessions, user);
    }

    public async Task<IEnumerable<AvailabilityCreatedEvent>> GetAvailabilityCreatedEventsAsync(string site, DateOnly from)
    {
        var events = await availabilityCreatedEventStore.GetAvailabilityCreatedEvents(site);

        return events
            .Where(acEvent => (acEvent.To ?? acEvent.From) >= from)
            .OrderBy(e => e.From)
            .ThenBy(e => e.To);
    }

    public async Task SetAvailabilityAsync(DateOnly date, string site, Session[] sessions, ApplyAvailabilityMode mode,
        Session sessionToEdit = null)
    {
        if (string.IsNullOrEmpty(site))
            throw new ArgumentException("Site must have a value.");

        if (sessions is null || sessions.Length == 0)
            throw new ArgumentException("Availability must contain one or more sessions.");

        if (mode is ApplyAvailabilityMode.Edit && sessionToEdit is null)
        {
            throw new ArgumentException("When editing a session a session to edit must be supplied.");
        }

        await availabilityStore.ApplyAvailabilityTemplate(site, date, sessions, mode, sessionToEdit);
    }

    public async Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to)
    {
        return await availabilityStore.GetDailyAvailability(site, from, to);
    }

    public async Task CancelSession(string site, DateOnly date, string from, string until, string[] services, int slotLength, int capacity)
    {
        var sessionToCancel = new Session
        {
            Capacity = capacity,
            From = TimeOnly.Parse(from),
            Until = TimeOnly.Parse(until),
            Services = services,
            SlotLength = slotLength,
        };

        await availabilityStore.CancelSession(site, date, sessionToCancel);
    }

    private static IEnumerable<DateOnly> GetDatesBetween(DateOnly start, DateOnly end, params DayOfWeek[] weekdays)
    {
        var cursor = start;
        while (cursor <= end)
        {
            if (weekdays.Contains(cursor.DayOfWeek))
                yield return cursor;
            cursor = cursor.AddDays(1);
        }
    }

    public SessionInstance ChooseHighestPrioritySlot(List<SessionInstance> slots, Booking booking) =>
        slots.Where(
                sl => sl.Capacity > 0
                      && sl.From == booking.From
                      && (int)sl.Duration.TotalMinutes == booking.Duration
                      && sl.Services.Contains(booking.Service))
            .OrderBy(slot => slot.Services.Length)
            .ThenBy(slot => string.Join(string.Empty, slot.Services.Order()))
            .FirstOrDefault();

    public async Task<AvailabilityState> GetAvailabilityState(string site, DateOnly day)
    {
        var availabilityState = new AvailabilityState();

        var orderedLiveBookings = await GetOrderedLiveBookings(site, day);
        var slots = await GetSlots(site, day);

        using var leaseContent = siteLeaseManager.Acquire(site);

        foreach (var booking in orderedLiveBookings)
        {
            var targetSlot = ChooseHighestPrioritySlot(slots, booking);
            var bookingIsSupportedByAvailability = targetSlot is not null;

            if (bookingIsSupportedByAvailability)
            {
                if (booking.AvailabilityStatus is not AvailabilityStatus.Supported)
                {
                    availabilityState.Recalculations.Add(new AvailabilityUpdate(booking,
                        AvailabilityUpdateAction.SetToSupported));
                }

                targetSlot.Capacity--;
                continue;
            }

            if (booking.AvailabilityStatus is AvailabilityStatus.Supported &&
                booking.Status is not AppointmentStatus.Provisional)
            {
                availabilityState.Recalculations.Add(new AvailabilityUpdate(booking,
                    AvailabilityUpdateAction.SetToOrphaned));
            }

            if (booking.Status is AppointmentStatus.Provisional)
            {
                availabilityState.Recalculations.Add(new AvailabilityUpdate(booking,
                    AvailabilityUpdateAction.ProvisionalToDelete));
            }
        }

        availabilityState.AvailableSlots = slots.Where(s => s.Capacity > 0).ToList();
        availabilityState.Bookings = orderedLiveBookings;

        return availabilityState;
    }

    public async Task<AvailabilityState> RecalculateAppointmentStatuses(string site, DateOnly day)
    {
        var availabilityState = await GetAvailabilityState(site, day);

        foreach (var update in availabilityState.Recalculations)
        {
            switch (update.Action)
            {
                case AvailabilityUpdateAction.ProvisionalToDelete:
                    await bookingsService.DeleteBooking(update.Booking.Reference, update.Booking.Site);
                    break;

                case AvailabilityUpdateAction.SetToOrphaned:
                    await bookingsService.UpdateAvailabilityStatus(update.Booking.Reference,
                        AvailabilityStatus.Orphaned);
                    break;

                case AvailabilityUpdateAction.SetToSupported:
                    await bookingsService.UpdateAvailabilityStatus(update.Booking.Reference,
                        AvailabilityStatus.Supported);
                    break;

                case AvailabilityUpdateAction.Default:
                default:
                    break;
            }
        }

        return new AvailabilityState
        {
            AvailableSlots = availabilityState.AvailableSlots, Bookings = availabilityState.Bookings
        };
    }
}
