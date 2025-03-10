using Newtonsoft.Json;
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

    internal async Task<List<Booking>> GetOrderedLiveBookings(string site, DateOnly day)
    {
        var dayStart = day.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = day.ToDateTime(new TimeOnly(23, 59));

        var bookings = (await bookingsService.GetBookings(dayStart, dayEnd, site))
            .Where(b => _liveStatuses.Contains(b.Status))
            .OrderBy(b => b.From)
            .ThenBy(b=> b.Duration)
            .ThenBy(b => b.Created)
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
                if (TemporaryFeatureToggles.MultiServiceAvailabilityCalculationsV2)
                {
                    await RecalculateAppointmentStatusesV2(site, date);
                }
                
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
            if (TemporaryFeatureToggles.MultiServiceAvailabilityCalculationsV2)
            {
                await RecalculateAppointmentStatusesV2(site, date);
            }
            
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
            .OrderBy(slot => slot.Services.Count)
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

    public async Task<AvailabilityState> GetAvailabilityStateV2(string site, DateOnly day)
    {
        var availabilityState = new AvailabilityState();

        var orderedLiveBookings = await GetOrderedLiveBookings(site, day);
        var slots = await GetSlots(site, day);

        var groupedBookingSlots = orderedLiveBookings
            .GroupBy(slot => new { slot.From, slot.Duration });

        //apply logic to assign booking slots into session slots
        //i.e lets just consider all the bookings from 9:00 to 9:10 (as example), and all the slots offered for this time
        foreach (var groupedBookingSlot in groupedBookingSlots)
        {
            var allBookings = groupedBookingSlot.ToList();
            var allSlots = slots.Where(x => x.From == groupedBookingSlot.Key.From && x.Duration == TimeSpan.FromMinutes(groupedBookingSlot.Key.Duration)).ToList();
            
            //generate some random internal IDs??
            foreach (var slot in allSlots)
            {
                slot.InternalId = Guid.NewGuid();
            }

            //go and get all the possible services offered by all slots in the 9:00 to 9:10 range
            var allServicesOfferedInTheseSlots = allSlots.SelectMany(x => x.Services).Distinct().ToList();
            
            //group by service offering also?
            var groupedSlotsByServiceCount = allSlots.GroupBy(slot => new { slot.Services.Count }).Order();

            //loop through the slots by their session service array length (i.e slots that only offer one service, then two, etc...)
            foreach (var slotsByServiceCount in groupedSlotsByServiceCount)
            {
                //now that we've got the bookingPriorityDictionary order, we will use this to remove the lowest priority
                //item with the lowest fractions when making decisions!
                foreach (var booking in allBookings)
                {
                    //go and get all bookings left 
                    var remainingBookings = groupedBookingSlot.Where(x => !x.Done).ToList();
                    //go and reevaluate what slots are remaining to use
                    var allRemainingSlots = slotsByServiceCount.Where(x => x.Capacity > 0).ToList();
                
                    //based on all available services from 9:00 - 9:10, and for slots of this service length, what are the current totals?
                    var slotTotalsDictionary = new Dictionary<string, decimal>();
                    var bookingPriorityDictionary = new Dictionary<string, decimal>();
                
                    foreach (var service in allServicesOfferedInTheseSlots)
                    {
                        //ignore for the service we're booking for
                        if (service == booking.Service)
                        {
                            continue;
                        }
                        
                        slotTotalsDictionary.Add(service, allRemainingSlots.Count(x => x.Services.Contains(service)));
                    }
                    
                    //recalculate booking priority dictionary
                    var bookingTotalsDictionary = new Dictionary<string, decimal>();
                    foreach (var service in allServicesOfferedInTheseSlots)
                    {
                        //ignore for the service we're booking for
                        if (service == booking.Service)
                        {
                            continue;
                        }
                        
                        bookingTotalsDictionary.Add(service, remainingBookings.Count(x => x.Service.Equals(service)));
                    }
                    
                    foreach (var service in allServicesOfferedInTheseSlots)
                    {
                        //ignore for the service we're booking for
                        if (service == booking.Service)
                        {
                            continue;
                        }
                        
                        var bookingTotalForService = bookingTotalsDictionary[service];
                        var slotsTotalForService = slotTotalsDictionary[service];

                        if (slotsTotalForService == 0)
                        {
                            bookingPriorityDictionary.Add(service, 0);
                        }
                        else
                        {
                            bookingPriorityDictionary.Add(service, (bookingTotalForService / slotsTotalForService));
                        }
                    }
                    
                    //need to deep clone remaining slot state for each booking
                    //this is required as we are going to alter state of remaining slots for this calculation
                    var clonedSlots = JsonConvert.DeserializeObject<List<SessionInstance>>(
                        JsonConvert.SerializeObject(allRemainingSlots)
                    );
                    
                    var orderedPriority = bookingPriorityDictionary.OrderBy(x => x.Value).ToList();
                    
                    //when deciding which slot to take, we need to iterate through services offered
                    //and remove slots that offer services in lowest remainingAvailability order
                    var serviceIterations = slotsByServiceCount.Key.Count;

                    SessionInstance clonedTargetSlot = null;

                    // var servicesRemoved = new List<string>();
                    
                    //this should reduce all slots down service by service until only one service remains (the one we want to book)
                    for (var i = 0; i < serviceIterations; i++)
                    {
                        var nextLowestService = orderedPriority[i].Key;
                        
                        //need to do it in a way here that doesn't impact overall slots for future calculations
                        foreach (var clonedSlot in clonedSlots)
                        {
                            clonedSlot.Services.Remove(nextLowestService);
                        }
                        
                        // //TODO HOW DO WE KNOW WHAT SERVICES THE FOUND SLOT HAD!!!
                        // servicesRemoved.Add(nextLowestService);
                        
                        //try and find a matching slot with exactly the one service!!
                        clonedTargetSlot = clonedSlots.FirstOrDefault(x => x.Services.Count == 1 && x.Services.Contains(booking.Service));

                        if (clonedTargetSlot != null)
                        {
                            break;
                        }
                    }

                    SessionInstance targetSlot = null;

                    //since we found a slot, need to add back in all the removed services we used to find it
                    //and get the real slot to use
                    if (clonedTargetSlot != null)
                    {
                        // clonedTargetSlot.Services.AddRange(servicesRemoved);
                        targetSlot = allRemainingSlots.SingleOrDefault(x => x.InternalId == clonedTargetSlot.InternalId);
                    }
                    
                    booking.Done = true;
                    
                    var bookingIsSupportedByAvailability = targetSlot is not null;
        
                    if (bookingIsSupportedByAvailability)
                    {
                        if (booking.AvailabilityStatus is not AvailabilityStatus.Supported)
                        {
                            availabilityState.Recalculations.Add(new AvailabilityUpdate(booking,
                                AvailabilityUpdateAction.SetToSupported));
                        }
        
                        targetSlot.Capacity--;
                        availabilityState.Bookings.Add(booking);
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
        }

        availabilityState.AvailableSlots = slots.Where(s => s.Capacity > 0).ToList();

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
    
    public async Task<AvailabilityState> RecalculateAppointmentStatusesV2(string site, DateOnly day)
    {
        var availabilityState = await GetAvailabilityStateV2(site, day);

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

        return new AvailabilityState
        {
            AvailableSlots = availabilityState.AvailableSlots, Bookings = availabilityState.Bookings
        };
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

        await RecalculateAppointmentStatuses(booking.Site, DateOnly.FromDateTime(booking.From));

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
            var slots = (await GetAvailabilityState(booking.Site,
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
}
