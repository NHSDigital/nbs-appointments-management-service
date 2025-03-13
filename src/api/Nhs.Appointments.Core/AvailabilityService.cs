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

        await RecalculateAppointmentStatuses(booking.Site, DateOnly.FromDateTime(booking.From),
            DateOnly.FromDateTime(booking.From));

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
            var slots = (await GetAvailabilityState(booking.Site, DateOnly.FromDateTime(booking.From.Date),
                DateOnly.FromDateTime(booking.From.Date))).AvailableSlots;

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

    private record OpportunityCostMetric(decimal BookingsOverSlots, int UpcomingBookingToProcess);

    public async Task<AvailabilityState> GetAvailabilityState(string site, DateOnly from, DateOnly to,
        string serviceToQuery = null)
    {
        var availabilityState = new AvailabilityState();

        var orderedLiveBookings = await GetOrderedLiveBookings(site, from, to);

        var slots = await GetSlots(site, from, to);

        var groupedSlotsDictionary = slots.GroupBy(x => new { x.From, x.Duration })
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

            groupedSlotsDictionary.TryGetValue(
                new { groupedBookingSlot.Key.From, Duration = TimeSpan.FromMinutes(groupedBookingSlot.Key.Duration), },
                out var allSlots);

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
                var remainingBookings = allBookings.Skip(i).ToList();

                //go and reevaluate what slots are remaining to use
                var remainingValidSlots = allSlots
                    .Where(x => x.Capacity > 0 && x.Services.Contains(booking.Service))
                    .ToList();

                //very first check, is there only a single slot remaining that this booking could go in, regardless of other services in that slot
                var targetSlot = remainingValidSlots.Count(x => x.Services.Contains(booking.Service)) == 1
                    ? remainingValidSlots.Single(x => x.Services.Contains(booking.Service))
                    : null;

                //more than one candidate, continue...
                if (targetSlot == null)
                {
                    //check if there are any slots where only offer single service that we need to book (no need to attempt priority check)
                    //just take the first such slot if it exists
                    targetSlot = remainingValidSlots.FirstOrDefault(x => x.Services.Length == 1);

                    //if we couldn't find a single slot, try and find the next possible slot with best fit
                    if (targetSlot == null)
                    {
                        var opportunityCostDictionary = new Dictionary<string, OpportunityCostMetric>();
                        
                        var remainingUpcomingBookingServices = remainingBookings
                            .Where(x => x.Service != booking.Service)
                            .Select(x => x.Service)
                            .Distinct()
                            .ToList();

                        foreach (var service in allServicesOfferedInTheseSlots)
                        {
                            //ignore for the service we're booking for
                            if (service == booking.Service)
                            {
                                continue;
                            }

                            decimal totalRemainingSlotsCapacity = remainingValidSlots
                                .Where(x => x.Services.Contains(service))
                                .Sum(x => x.Capacity);

                            //if any remaining slots left, add to dictionary for consideration
                            if (totalRemainingSlotsCapacity > 0)
                            {
                                decimal totalRemainingBookings =
                                    remainingBookings.Count(x => x.Service.Equals(service));
                                
                                opportunityCostDictionary.Add(service, 
                                    new OpportunityCostMetric(
                                        Math.Min(1, totalRemainingBookings / totalRemainingSlotsCapacity), //first metric, limit to 1 (don't let oversubscribed bookings skew the data)
                                        remainingUpcomingBookingServices.IndexOf(service) //second
                                    ));
                            }
                        }

                        var orderedOpportunityCost = opportunityCostDictionary
                            //first order by the opportunity cost metric
                            .OrderBy(x => x.Value.BookingsOverSlots)
                            //the further down the remaining bookings list a service is, the lower the opportunity cost
                            .ThenByDescending(x => x.Value.UpcomingBookingToProcess)
                            //then by alphabetical for determinism if all else equal
                            .ThenBy(x => x.Key)
                            .ToList();

                        var bestFitCandidateServices = new List<string> { booking.Service };
                        
                        //TODO performance - can we start the loop at the min services remaining left across all remainingSlots
                        //i.e if all remaining slots left have minimum length of 10, why would we bother doing the first 9 loops of this orderedOpportunityCost
                        //as the subset check can't possibly pass either of the 9 times
 
                        //this should reduce all slots down service by service until only one service remains (the one we want to book)
                        foreach (var opportunityCost in orderedOpportunityCost)
                        {
                            var nextLowestService = opportunityCost.Key;
                            bestFitCandidateServices.Add(nextLowestService);

                            //TODO add short circuit if none of the remainingValidSlots service lengths are >= bestFitCandidateServices length
                            //as the subset check can't possibly pass

                            //TODO need to order the remainingValidSlots be service length descending if multiple found??
                            
                            //a valid slots services must be a subset of the bestFitCandidateServices
                            targetSlot = remainingValidSlots.FirstOrDefault(x =>
                                x.Services.Intersect(bestFitCandidateServices).Count() == x.Services.Length);

                            if (targetSlot != null)
                            {
                                break;
                            }
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
