using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Core;

public class BookingAvailabilityStateService(
    IAvailabilityQueryService availabilityQueryService,
    IBookingQueryService bookingQueryService,
    ILogger<BookingAvailabilityStateService> logger
) : IBookingAvailabilityStateService
{
    private readonly IReadOnlyList<AppointmentStatus> _liveStatuses =
        [AppointmentStatus.Booked, AppointmentStatus.Provisional];

    public async Task<Summary> GetWeekSummary(string site, DateOnly from)
    {
        var dayStart = from.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = from.AddDays(6).ToDateTime(new TimeOnly(23, 59, 59));

        return (await BuildState(site, dayStart, dayEnd, BookingAvailabilityStateReturnType.Summary))
            .Summary;
    }

    public async Task<Summary> GetDaySummary(string site, DateOnly day)
    {
        var dayStart = day.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = day.ToDateTime(new TimeOnly(23, 59, 59));
        
        return (await BuildState(site, dayStart, dayEnd, BookingAvailabilityStateReturnType.Summary))
            .Summary;
    }

    public async Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from,
        DateTime to)
    {
        return (await BuildState(site, from, to, BookingAvailabilityStateReturnType.Recalculations))
            .BookingAvailabilityUpdates;
    }

    /// <summary>
    ///     Use short-circuit logic to try and return a truthy as quick as possible
    ///     Expectation that the majority of cases will return a result instead of needing to build up the full state model
    ///     Includes service session existence check, an empty slot existence check and finally an inverse-pigeonhole principle
    ///     check.
    /// </summary>
    public async Task<(bool hasSlot, bool shortCircuited)> HasAnyAvailableSlot(string service, string site,
        DateOnly from, DateOnly to)
    {
        //we want to order slots and bookings DESCENDING as likely to have more availability in the future (bookings tend to occur closer to the present based on real data)
        //we're more likely to find an empty/available slot quicker if we start from the furthest point in the future
        var sessionsForService = (await availabilityQueryService.GetSessionsForServiceDescending(
            site,
            service,
            from,
            to)).ToList();

        //try and find a result as quickly as possible, lets look at bookings and slots in the latest week first
        //this means fetching less data from the DB, and less processing to do
        var weekPartitionsDesc = GetWeekPartitions(from, to).OrderByDescending(x => x.To);
        
        //fetch sessions first and check there exists documents that support it, else has no capacity for that service
        if (sessionsForService.Count == 0)
        {
            logger.LogInformation(
                "HasAnyAvailableSlot short circuit success - No sessions for service: '{Service}', site : '{Site}', from : '{From}', to : '{To}'.",
                service, site, from, to);
            return (false, true);
        }

        //only care about short-circuiting for slots that can support our queried service
        var slotsForService = sessionsForService
            .SelectMany(session => session.ToSlots())
            .Where(x => x.Services.Contains(service));
        
        //loop through each weekly partition
        //possible multiple DB calls but expecting an early result
        foreach (var weekPartition in weekPartitionsDesc)
        {
            var weekStart = weekPartition.From.ToDateTime(new TimeOnly(0, 0, 0));
            var weekEnd = weekPartition.To.ToDateTime(new TimeOnly(23, 59, 59));

            var slotsInWeek = slotsForService
                .Where(x => x.From >= weekStart && x.From.Add(x.Duration) <= weekEnd).ToList();
            
            //only need to return bookings that are also contained in slots that support our queried service
            //i.e we don't need to return bookings for services that can't be allocated to any of our relevant slots
            var allSupportedServices = slotsInWeek.SelectMany(x => x.Services).Distinct().ToArray();
            var bookingsInWeek = (await bookingQueryService.GetLiveBookings(site, weekStart, weekEnd)).Where(x => allSupportedServices.Contains(x.Service)).ToList();

            var emptySlotExists = AnEmptySlotExists(slotsInWeek, bookingsInWeek);
            if (emptySlotExists)
            {
                logger.LogInformation(
                    "HasAnyAvailableSlot short circuit success - Empty slot exists for service: '{Service}', site : '{Site}', from : '{From}', to : '{To}'.",
                    service, site, from, to);
                return (true, true);
            }
            
            var slotMustExist = ASlotWithSpaceMustExist(slotsInWeek, bookingsInWeek);
            if (slotMustExist)
            {
                logger.LogInformation(
                    "HasAnyAvailableSlot short circuit success - Guaranteed slot with capacity exists for service: '{Service}', site : '{Site}', from : '{From}', to : '{To}'.",
                    service, site, from, to);
                return (true, true);
            }
        }
        
        logger.LogInformation(
            "HasAnyAvailableSlot short circuit attempt unsuccessful - falling back to building the full state to ascertain the availability for service: '{Service}', site : '{Site}', from : '{From}', to : '{To}'.",
            service, site, from, to);

        //fallback to building up full state to get a definite decision
        var availableSlots = await GetAvailableSlots(site, from.ToDateTime(new TimeOnly(0, 0, 0)), to.ToDateTime(new TimeOnly(23, 59, 59)));

        //return if any of the available slots for the full state model can support our queried service
        return (availableSlots.Any(sl => sl.Services.Contains(service)), false);
    }

    private bool ASlotWithSpaceMustExist(IEnumerable<SessionInstance> slotsInWeek, IEnumerable<Booking> bookingsInWeek)
    {
        //we have ascertained no empty slots exist, now to populate our dictionary with metrics for the inverse pigeonhole check
            var bookingTimeDictionary = new Dictionary<DateRange, Dictionary<string, int>>();

            foreach (var group in bookingsInWeek.GroupBy(x => new DateRange(x.From, x.From.AddMinutes(x.Duration))))
            {
                var serviceCounts = group
                    .GroupBy(b => b.Service)
                    .ToDictionary(
                        sg => sg.Key,
                        sg => sg.Count()
                    );

                bookingTimeDictionary[group.Key] = serviceCounts;
            }

            //want to group equivalent slots together for total capacity at that time-range and supported services
            var groupedSlotsForService = slotsInWeek
                .GroupBy(x => new SessionInstanceKey(x.From, x.Until, x.Services))
                .Select(g => new SessionInstance(g.Key.From, g.Key.Until)
                {
                    Services = g.Key.Services,
                    //Total capacity
                    Capacity = g.Sum(x => x.Capacity)
                })
                .ToList();

            //does a slot exist where the number of total bookings for the services in that session is less than the total capacity for that slot?
            //even if ALL the available bookings that slot can support are allocated to that slot, there is still > 0 capacity remaining
            //if this is true, then the allocation algorithm used in BuildState is irrelevant, it is impossible to fill the slot based on the current live bookings
            return groupedSlotsForService.Any(slotGrouping =>
            {
                var bookingsForThatSlot = bookingTimeDictionary[new DateRange(slotGrouping.From, slotGrouping.Until)];

                //only total the bookings that the slot can support
                var bookingServiceTotal = bookingsForThatSlot.Where(x => slotGrouping.Services.Contains(x.Key)).Sum(x => x.Value);

                return slotGrouping.Capacity > bookingServiceTotal;
            });
    }

    private bool AnEmptySlotExists(IEnumerable<SessionInstance> slotsInWeek, IEnumerable<Booking> bookingsInWeek)
    {
        var distinctBookingTimes = bookingsInWeek
            .Select(x => new DateRange(x.From, x.From.AddMinutes(x.Duration)))
            .Distinct();

        //does a slot exist where no bookings exist in the DB at that time?
        return slotsInWeek.Any(slot => !distinctBookingTimes.Contains(new DateRange(slot.From, slot.Until)));
    }

    public static List<DateOnlyRange> GetWeekPartitions(DateOnly from, DateOnly to)
    {
        var result = new List<DateOnlyRange>();
        
        var daysDifference = to.DayNumber - from.DayNumber;

        switch (daysDifference)
        {
            case < 0:
                return [];
            case <= 7:
                result.Add(new DateOnlyRange(from, to));
                return result;
        }

        var current = from;

        while (current.AddDays(6) < to)
        {
            result.Add(new DateOnlyRange(current, current.AddDays(6)));
            current = current.AddDays(7);
        }

        // Add final range for remaining days, ending exactly on 'to'
        if (current <= to)
        {
            result.Add(new DateOnlyRange(current, to));
        }

        return result;
    }

    public async Task<IEnumerable<SessionInstance>> GetAvailableSlots(string site, DateTime from, DateTime to)
    {
        return (await BuildState(site, from, to, BookingAvailabilityStateReturnType.AvailableSlots)).AvailableSlots;
    }

    private async Task<(IEnumerable<Booking> bookings, List<LinkedSessionInstance> sessions)> FetchData(string site,
        DateTime from, DateTime to, BookingAvailabilityStateReturnType returnType)
    {
        var isSummary = returnType == BookingAvailabilityStateReturnType.Summary;

        var statuses = isSummary ? [AppointmentStatus.Booked, AppointmentStatus.Cancelled] : _liveStatuses;
        var bookingsTask = bookingQueryService.GetOrderedBookings(site, from, to, statuses);
        var sessionsTask = availabilityQueryService.GetLinkedSessions(site, DateOnly.FromDateTime(from),
            DateOnly.FromDateTime(to), isSummary);
        await Task.WhenAll(bookingsTask, sessionsTask);
        return (bookingsTask.Result, sessionsTask.Result.ToList());
    }

    private static IEnumerable<SessionSummary> GetDailySessionSummaries(DateOnly date,
        List<LinkedSessionInstance> sessionInstances)
    {
        return sessionInstances.Where(x => DateOnly.FromDateTime(x.From).Equals(date)).Select(x => new SessionSummary
        {
            Id = x.InternalSessionId!.Value,
            UkStartDatetime = x.From,
            UkEndDatetime = x.Until,
            MaximumCapacity = x.Capacity * x.ToSlots().Count(),
            Capacity = x.Capacity,
            SlotLength = x.SlotLength,
            Bookings = x.Services.ToDictionary(key => key, _ => 0)
        }).ToList();
    }

    private async Task<BookingAvailabilityState> BuildState(string site, DateTime from, DateTime to,
        BookingAvailabilityStateReturnType returnType)
    {
        var (bookings, sessions) =
            await FetchData(site, from, to, returnType);
        var state = new BookingAvailabilityState();

        //have to materialise to a list as we transform the data within
        var slotsList = sessions.SelectMany(session => session.ToSlots()).ToList();

        var liveBookings = bookings.Where(x => _liveStatuses.Contains(x.Status));

        List<DaySummary> daySummaries = [];

        if (returnType == BookingAvailabilityStateReturnType.Summary)
        {
            daySummaries = InitialiseDaySummaries(from, to, sessions);
        }

        foreach (var booking in liveBookings)
        {
            var targetSlot = ChooseHighestPrioritySlot(slotsList, booking);
            var bookingIsSupportedByAvailability = targetSlot is not null;

            if (bookingIsSupportedByAvailability)
            {
                switch (returnType)
                {
                    case BookingAvailabilityStateReturnType.AvailableSlots:
                        break;
                    case BookingAvailabilityStateReturnType.Recalculations:
                        state.BookingAvailabilityUpdates.AppendNewlySupportedBooking(booking);
                        break;
                    case BookingAvailabilityStateReturnType.Summary:
                        //update status if not already supported

                        if (booking.AvailabilityStatus != AvailabilityStatus.Supported)
                        {
                            booking.AvailabilityStatus = AvailabilityStatus.Supported;
                        }

                        var fromDate = DateOnly.FromDateTime(targetSlot.From);
                        var sessionToUpdate = daySummaries.Where(x => x.Date == fromDate)
                            .SelectMany(x => x.Sessions)
                            .Single(x => x.Id == targetSlot.InternalSessionId);

                        sessionToUpdate.Bookings[booking.Service]++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(returnType), returnType, null);
                }

                targetSlot.Capacity--;
                continue;
            }

            switch (returnType)
            {
                case BookingAvailabilityStateReturnType.AvailableSlots:
                    continue;
                case BookingAvailabilityStateReturnType.Summary:
                    if (booking.AvailabilityStatus is AvailabilityStatus.Supported &&
                        booking.Status is AppointmentStatus.Booked)
                    {
                        booking.AvailabilityStatus = AvailabilityStatus.Orphaned;
                    }

                    continue;
                case BookingAvailabilityStateReturnType.Recalculations:
                    state.BookingAvailabilityUpdates.AppendNoLongerSupportedBookings(booking);
                    state.BookingAvailabilityUpdates.AppendProvisionalBookingsToBeDeleted(booking);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(returnType), returnType, null);
            }
        }

        switch (returnType)
        {
            case BookingAvailabilityStateReturnType.AvailableSlots:
                //we only need to do this calculation if we want to return the available slots
                state.AvailableSlots = slotsList.Where(s => s.Capacity > 0);
                break;
            case BookingAvailabilityStateReturnType.Summary:
                state.Summary = GenerateSummary(bookings, daySummaries);
                break;
            case BookingAvailabilityStateReturnType.Recalculations:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(returnType), returnType, null);
        }

        return state;
    }

    private static Summary GenerateSummary(IEnumerable<Booking> bookings, List<DaySummary> daySummaries)
    {
        var orphaned = new Dictionary<string, int>();
        foreach (var daySummary in daySummaries)
        {
            daySummary.MaximumCapacity = daySummary.Sessions.Sum(x => x.MaximumCapacity);
            daySummary.RemainingCapacity = daySummary.Sessions.Sum(x => x.RemainingCapacity);

            var bookingsOnDay = bookings.Where(x => DateOnly.FromDateTime(x.From) == daySummary.Date).ToList();

            foreach (var booking in bookingsOnDay)
            {
                switch (booking.Status)
                {
                    case AppointmentStatus.Booked:
                        switch (booking.AvailabilityStatus)
                        {
                            case AvailabilityStatus.Supported:
                                daySummary.BookedAppointments++;
                                break;
                            case AvailabilityStatus.Orphaned:
                                orphaned[booking.Service] = orphaned.GetValueOrDefault(booking.Service, 0) + 1;
                                daySummary.OrphanedAppointments++;
                                break;
                        }

                        break;
                    case AppointmentStatus.Cancelled:
                        daySummary.CancelledAppointments++;
                        break;
                }
            }
        }

        return new Summary
        {
            DaySummaries = daySummaries,
            Orphaned = orphaned,
            MaximumCapacity = daySummaries.Sum(x => x.MaximumCapacity),
            RemainingCapacity = daySummaries.Sum(x => x.RemainingCapacity),
            BookedAppointments = daySummaries.Sum(x => x.BookedAppointments),
            OrphanedAppointments = orphaned.Sum(x => x.Value)
        };
    }

    private static List<DaySummary> InitialiseDaySummaries(DateTime from, DateTime to,
        List<LinkedSessionInstance> sessions)
    {
        var dayDate = DateOnly.FromDateTime(from.Date);

        List<DaySummary> daySummaries =
        [
            new(dayDate, GetDailySessionSummaries(dayDate, sessions))
        ];

        while (dayDate < DateOnly.FromDateTime(to.Date))
        {
            dayDate = dayDate.AddDays(1);
            daySummaries.Add(new DaySummary(dayDate, GetDailySessionSummaries(dayDate, sessions)));
        }

        return daySummaries;
    }

    /// <summary>
    ///     'Greedy' solution for assigning bookings to slots for multiple services in a deterministic way
    /// </summary>
    /// <param name="slots"></param>
    /// <param name="booking"></param>
    /// <returns></returns>
    private static LinkedSessionInstance ChooseHighestPrioritySlot(IEnumerable<LinkedSessionInstance> slots,
        Booking booking) =>
        slots.Where(sl => sl.Capacity > 0
                          && sl.From == booking.From
                          && (int)sl.Duration.TotalMinutes == booking.Duration
                          && sl.Services.Contains(booking.Service))
            .OrderBy(slot => slot.Services.Length)
            .ThenBy(slot => string.Join(string.Empty, slot.Services.Order()))
            .FirstOrDefault();
}

public readonly record struct DateRange(DateTime From, DateTime To);

public readonly record struct DateOnlyRange(DateOnly From, DateOnly To);

public sealed class SessionInstanceKey : IEquatable<SessionInstanceKey>
{
    public SessionInstanceKey(DateTime from, DateTime until, IEnumerable<string> services)
    {
        From = from;
        Until = until;
        Services = services.OrderBy(s => s).ToArray();
    }

    public DateTime From { get; }
    public DateTime Until { get; }
    public string[] Services { get; }

    public bool Equals(SessionInstanceKey? other)
    {
        if (other is null)
        {
            return false;
        }

        return From == other.From &&
               Until == other.Until &&
               Services.SequenceEqual(other.Services);
    }

    public override bool Equals(object? obj) => Equals(obj as SessionInstanceKey);

    public override int GetHashCode()
    {
        var hash = HashCode.Combine(From, Until);
        return Services.Aggregate(hash, HashCode.Combine);
    }
}

public static class RecalculationExtensions
{
    public static void AppendNewlySupportedBooking(this List<BookingAvailabilityUpdate> recalculations, Booking booking)
    {
        if (booking.AvailabilityStatus is not AvailabilityStatus.Supported)
        {
            recalculations.Add(
                new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.SetToSupported));
        }
    }

    public static void AppendNoLongerSupportedBookings(this List<BookingAvailabilityUpdate> recalculations,
        Booking booking)
    {
        if (booking.AvailabilityStatus is AvailabilityStatus.Supported &&
            booking.Status is AppointmentStatus.Booked)
        {
            recalculations.Add(
                new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.SetToOrphaned));
        }
    }

    public static void AppendProvisionalBookingsToBeDeleted(this List<BookingAvailabilityUpdate> recalculations,
        Booking booking)
    {
        if (booking.Status is AppointmentStatus.Provisional)
        {
            recalculations.Add(new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.ProvisionalToDelete));
        }
    }
}

public class BookingAvailabilityState
{
    public readonly List<BookingAvailabilityUpdate> BookingAvailabilityUpdates = [];

    public IEnumerable<SessionInstance> AvailableSlots { get; set; } = [];

    public Summary Summary { get; set; }
}

public enum BookingAvailabilityStateReturnType
{
    /// <summary>
    ///     Return a list of available slots
    /// </summary>
    AvailableSlots = 0,

    /// <summary>
    ///     Return a list of bookings that need an update
    /// </summary>
    Recalculations = 1,

    /// <summary>
    ///     Return a summary of booking/availability for a period
    /// </summary>
    Summary = 2
}

public class BookingAvailabilityUpdate(Booking booking, AvailabilityUpdateAction action)
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
