using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core;

public class AvailabilityService(
    IAvailabilityStore availabilityStore,
    IAvailabilityCreatedEventStore availabilityCreatedEventStore,
    IBookingsService bookingsService,
    ISiteLeaseManager siteLeaseManager,
    IBookingsDocumentStore bookingDocumentStore,
    IReferenceNumberProvider referenceNumberProvider,
    IBookingEventFactory eventFactory,
    IMessageBus bus,
    TimeProvider time) : IAvailabilityService
{
    private readonly AppointmentStatus[] _liveStatuses = [AppointmentStatus.Booked, AppointmentStatus.Provisional];

    public async Task ApplyAvailabilityTemplateAsync(string site, DateOnly from, DateOnly until, Template template,
        ApplyAvailabilityMode mode, string user)
    {
        if (string.IsNullOrEmpty(site))
        {
            throw new ArgumentException("site must have a value");
        }

        if (from > until)
        {
            throw new ArgumentException("until date must be after from date");
        }

        if (template == null)
        {
            throw new ArgumentException("template must be provided");
        }

        if (template.Sessions is null || template.Sessions.Length == 0)
        {
            throw new ArgumentException("template must contain one or more sessions");
        }

        if (template.Days is null || template.Days.Length == 0)
        {
            throw new ArgumentException("template must specify one or more weekdays");
        }

        var dates = GetDatesBetween(from, until, template.Days);
        foreach (var date in dates)
        {
            await availabilityStore.ApplyAvailabilityTemplate(site, date, template.Sessions, mode);

            if (TemporaryFeatureToggles.MultiServiceAvailabilityCalculations)
            {
                await RecalculateAppointmentStatuses(site, date, date);
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
            await RecalculateAppointmentStatuses(site, date, date);
        }
        else
        {
            await bookingsService.RecalculateAppointmentStatuses(site, date);
        }

        await availabilityCreatedEventStore.LogSingleDateSessionCreated(site, date, sessions, user);
    }

    public async Task<IEnumerable<AvailabilityCreatedEvent>> GetAvailabilityCreatedEventsAsync(string site,
        DateOnly from)
    {
        var events = await availabilityCreatedEventStore.GetAvailabilityCreatedEvents(site);

        return events
            .Where(acEvent => (acEvent.To ?? acEvent.From) >= from)
            .OrderBy(e => e.From)
            .ThenBy(e => e.To);
    }

    public async Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to)
    {
        return await availabilityStore.GetDailyAvailability(site, from, to);
    }

    public async Task CancelSession(string site, DateOnly date, string from, string until, string[] services,
        int slotLength, int capacity)
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
    
    public async Task RecalculateAppointmentStatuses(string site, DateOnly from, DateOnly to)
    {
        var availabilityState = await GetAvailabilityState(site, from, to);

        using var leaseContent = siteLeaseManager.Acquire(site);

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
    }

    public async Task<BookingCancellationResult> CancelBooking(string bookingReference, string siteId)
    {
        var booking = await bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);

        if (booking is null || (!string.IsNullOrEmpty(siteId) && siteId != booking.Site))
        {
            return BookingCancellationResult.NotFound;
        }

        await bookingDocumentStore.UpdateStatus(bookingReference, AppointmentStatus.Cancelled,
            AvailabilityStatus.Unknown);

        await RecalculateAppointmentStatuses(booking.Site, DateOnly.FromDateTime(booking.From), DateOnly.FromDateTime(booking.From));

        if (booking.ContactDetails != null)
        {
            var bookingCancelledEvents = eventFactory.BuildBookingEvents<BookingCancelled>(booking);
            await bus.Send(bookingCancelledEvents);
        }

        return BookingCancellationResult.Success;
    }

    public async Task<(bool Success, string Reference)> MakeBooking(Booking booking)
    {
        using (var leaseContent = siteLeaseManager.Acquire(booking.Site))
        {
            //TODO improve performance by only querying state for the bookings exact From and To datetimes
            var slots = (await GetAvailabilityState(booking.Site, DateOnly.FromDateTime(booking.From.Date), DateOnly.FromDateTime(booking.From.Date))).AvailableSlots;

            var canBook = slots.Any(sl => sl.From == booking.From && sl.Duration.TotalMinutes == booking.Duration);

            if (canBook)
            {
                booking.Created = time.GetUtcNow();
                booking.Reference = await referenceNumberProvider.GetReferenceNumber(booking.Site);
                booking.ReminderSent = false;
                booking.AvailabilityStatus = AvailabilityStatus.Supported;
                await bookingDocumentStore.InsertAsync(booking);

                if (booking.Status == AppointmentStatus.Booked && booking.ContactDetails?.Length > 0)
                {
                    var bookingMadeEvents = eventFactory.BuildBookingEvents<BookingMade>(booking);
                    await bus.Send(bookingMadeEvents);
                }

                return (true, booking.Reference);
            }

            return (false, string.Empty);
        }
    }

    public async Task<AvailabilityState> GetAvailabilityState(string site, DateOnly from, DateOnly to,
        string serviceToQuery = null)
    {
        var availabilityState = new AvailabilityState();

        var orderedLiveBookings = await GetOrderedLiveBookings(site, from, to);

        var slots = await GetSlots(site, from, to);
        
        var slotsDictionary = slots.GroupBy(x => new { x.From, x.Duration })
            .ToDictionary(g => g.Key, g => g.ToList());

        var groupedBookingSlots = orderedLiveBookings
            .GroupBy(slot => new { slot.From, slot.Duration });

        //apply logic to assign booking slots into session slots
        //i.e lets just consider all the bookings from 9:00 to 9:10 (as example), and all the slots offered for this time
        foreach (var groupedBookingSlot in groupedBookingSlots)
        {
            var allBookings = groupedBookingSlot.ToList();

            if (serviceToQuery != null)
            {
                allBookings.Add(new Booking
                {
                    From = groupedBookingSlot.Key.From,
                    Duration = groupedBookingSlot.Key.Duration,
                    Service = serviceToQuery,
                    Created = DateTimeOffset.UtcNow.AddYears(100)
                });
            }
            
            slotsDictionary.TryGetValue(new {
                groupedBookingSlot.Key.From,
                Duration = TimeSpan.FromMinutes(groupedBookingSlot.Key.Duration),
            } , out var allSlots);

            allSlots ??= [];
            
            //go and get all the possible services offered by all slots in the 9:00 to 9:10 range
            var allServicesOfferedInTheseSlots = new HashSet<string>(allSlots.SelectMany(x => x.Services));

            for (var i = 0; i < allBookings.Count; i++)
            {
                var booking = allBookings[i];

                //if querying service and processing the final 'ghost' booking, don't do any of the actions
                if (serviceToQuery != null && i == allBookings.Count - 1)
                {
                    continue;
                }

                //go and get all bookings left (skipping the first i that have already been processed)
                var remainingBookings = allBookings.Skip(i);
                
                //go and reevaluate what slots are remaining to use
                var remainingSlots = allSlots.Where(x => x.Capacity > 0).ToArray();

                var opportunityCostDictionary = new Dictionary<string, decimal>();

                foreach (var service in allServicesOfferedInTheseSlots)
                {
                    //ignore for the service we're booking for
                    if (service == booking.Service)
                    {
                        continue;
                    }
                    
                    decimal totalRemainingSlotsCapacity = remainingSlots.Where(x => x.Services.Contains(service)).Sum(x => x.Capacity);
                    decimal totalRemainingBookings = remainingBookings.Count(x => x.Service.Equals(service));

                    //protect against divide by zero
                    if (totalRemainingSlotsCapacity == 0)
                    {
                        //TODO investigate knock-on effect of removing this from the dictionary
                        //put to the back of the queue
                        opportunityCostDictionary.Add(service, int.MaxValue);
                    }
                    else
                    {
                        opportunityCostDictionary.Add(service, totalRemainingBookings / totalRemainingSlotsCapacity);
                    }
                }

                var orderedOpportunityCost = opportunityCostDictionary.OrderBy(x => x.Value).ToList();

                //check first if there are any slots where only offer single service that we need to book (no need to attempt priority check)
                var targetSlot = remainingSlots.FirstOrDefault(x => x.Services.Length == 1 && x.Services.Contains(booking.Service));

                //if we couldn't find a single slot, try and find the next possible slot with best fit
                if (targetSlot == null)
                {
                    var bestFitCandidateServices = new List<string> { booking.Service };

                    //this should reduce all slots down service by service until only one service remains (the one we want to book)
                    foreach (var opportunityCost in orderedOpportunityCost)
                    {
                        var nextLowestService = opportunityCost.Key;
                        bestFitCandidateServices.Add(nextLowestService);

                        //a valid slots services must be a subset of the bestFitCandidateServices
                        //and it must contain the booking service
                        targetSlot = remainingSlots.FirstOrDefault(x =>
                            x.Services.Contains(booking.Service) &&
                            x.Services.Intersect(bestFitCandidateServices).Count() == x.Services.Length);

                        if (targetSlot != null)
                        {
                            break;
                        }
                    }
                }

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
        }

        availabilityState.AvailableSlots = slots.Where(s => s.Capacity > 0).ToList();

        return availabilityState;
    }

    internal async Task<List<Booking>> GetOrderedLiveBookings(string site, DateOnly from, DateOnly to)
    {
        var dayStart = from.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = to.ToDateTime(new TimeOnly(23, 59));

        var bookings = (await bookingsService.GetBookings(dayStart, dayEnd, site))
            .Where(b => _liveStatuses.Contains(b.Status))
            .OrderBy(b => b.From)
            .ThenBy(b => b.Duration)
            .ThenBy(b => b.Created)
            .ToList();

        return bookings;
    }

    private async Task<List<SessionInstance>> GetSlots(string site, DateOnly from, DateOnly to)
    {
        var sessionsOnThatDay =
            (await availabilityStore.GetSessions(site, from, to))
            .ToList();

        var slots = sessionsOnThatDay
            .SelectMany(session => session.ToSlots())
            .ToList();

        return slots;
    }

    public async Task SetAvailabilityAsync(DateOnly date, string site, Session[] sessions, ApplyAvailabilityMode mode,
        Session sessionToEdit = null)
    {
        if (string.IsNullOrEmpty(site))
        {
            throw new ArgumentException("Site must have a value.");
        }

        if (sessions is null || sessions.Length == 0)
        {
            throw new ArgumentException("Availability must contain one or more sessions.");
        }

        if (mode is ApplyAvailabilityMode.Edit && sessionToEdit is null)
        {
            throw new ArgumentException("When editing a session a session to edit must be supplied.");
        }

        await availabilityStore.ApplyAvailabilityTemplate(site, date, sessions, mode, sessionToEdit);
    }

    private static IEnumerable<DateOnly> GetDatesBetween(DateOnly start, DateOnly end, params DayOfWeek[] weekdays)
    {
        var cursor = start;
        while (cursor <= end)
        {
            if (weekdays.Contains(cursor.DayOfWeek))
            {
                yield return cursor;
            }

            cursor = cursor.AddDays(1);
        }
    }
}
