namespace Nhs.Appointments.Core;

public class BookingAvailabilityStateService(
    IAvailabilityQueryService availabilityQueryService,
    IBookingQueryService bookingQueryService) : IBookingAvailabilityStateService
{
    private readonly IReadOnlyList<AppointmentStatus> _liveStatuses = [AppointmentStatus.Booked, AppointmentStatus.Provisional];

    public async Task<WeekSummary> GetWeekSummary(string site, DateOnly from)
    {
        var dayStart = from.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = from.AddDays(6).ToDateTime(new TimeOnly(23, 59, 59));

        return (await BuildState(site, dayStart, dayEnd, BookingAvailabilityStateReturnType.WeekSummary))
            .WeekSummary;
    }

    public async Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from,
        DateTime to)
    {
        return (await BuildState(site, from, to, BookingAvailabilityStateReturnType.Recalculations))
            .BookingAvailabilityUpdates;
    }

    public async Task<List<SessionInstance>> GetAvailableSlots(string site, DateTime from, DateTime to)
    {
        return (await BuildState(site, from, to, BookingAvailabilityStateReturnType.AvailableSlots)).AvailableSlots;
    }

    private async Task<(List<Booking> bookings, List<SessionInstance> slots)> FetchData(string site,
        DateTime from, DateTime to, BookingAvailabilityStateReturnType returnType)
    {
        var isWeekSummary = returnType == BookingAvailabilityStateReturnType.WeekSummary;

        var statuses = _liveStatuses.ToList();

        if (isWeekSummary)
        {
            statuses.Add(AppointmentStatus.Cancelled);
        }

        var bookingsTask = bookingQueryService.GetOrderedBookings(site, from, to, statuses);
        var sessionsTask = availabilityQueryService.GetSessions(site, DateOnly.FromDateTime(from),
            DateOnly.FromDateTime(to), generateSessionId: isWeekSummary);
        await Task.WhenAll(bookingsTask, sessionsTask);
        return (bookingsTask.Result.ToList(), sessionsTask.Result.ToList());
    }

    private List<SessionSummary> GetDailySessionSummaries(DateOnly date,
        IEnumerable<SessionInstance> sessionInstances)
    {
        return sessionInstances.Where(x => DateOnly.FromDateTime(x.From).Equals(date)).Select(x => new SessionSummary
        {
            Id = x.InternalSessionId!.Value,
            From = x.From,
            Until = x.Until,
            MaximumCapacity = x.Capacity * x.ToSlots().Count(),
            ServiceBookings = x.Services.ToDictionary(key => key, _ => 0)
        }).ToList();
    }

    private async Task<BookingAvailabilityState> BuildState(string site, DateTime from, DateTime to,
        BookingAvailabilityStateReturnType returnType)
    {
        var (bookings, sessions) =
            await FetchData(site, from, to, BookingAvailabilityStateReturnType.WeekSummary);
        var state = new BookingAvailabilityState();

        var slotsList = sessions.SelectMany(session => session.ToSlots()).ToList();
        var liveBookings = bookings.Where(x => _liveStatuses.Contains(x.Status));

        List<DaySummary> daySummaries = [];

        if (returnType == BookingAvailabilityStateReturnType.WeekSummary)
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
                    case BookingAvailabilityStateReturnType.WeekSummary:

                        //update status if not already supported
                        if (booking.AvailabilityStatus != AvailabilityStatus.Supported)
                        {
                            booking.AvailabilityStatus = AvailabilityStatus.Supported;
                        }
                        
                        var fromDate = DateOnly.FromDateTime(targetSlot.From);
                        var sessionToUpdate = daySummaries.Where(x => x.Date == fromDate)
                            .SelectMany(x => x.SessionSummaries)
                            .Single(x => x.Id == targetSlot.InternalSessionId);

                        sessionToUpdate.ServiceBookings[booking.Service]++;
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
                case BookingAvailabilityStateReturnType.WeekSummary:
                    //TODO any reason why we update cancelled Status when no longer supported? does it matter??
                    if (booking.AvailabilityStatus is AvailabilityStatus.Supported &&
                        booking.Status is not AppointmentStatus.Provisional)
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
                state.AvailableSlots = slotsList.Where(s => s.Capacity > 0).ToList();
                break;
            case BookingAvailabilityStateReturnType.WeekSummary:
                state.WeekSummary = GenerateWeekSummary(bookings, daySummaries);
                break;
            case BookingAvailabilityStateReturnType.Recalculations:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(returnType), returnType, null);
        }

        return state;
    }

    private WeekSummary GenerateWeekSummary(List<Booking> bookings, List<DaySummary> daySummaries)
    {
        foreach (var daySummary in daySummaries)
        {
            daySummary.MaximumCapacity = daySummary.SessionSummaries.Sum(x => x.MaximumCapacity);
            daySummary.RemainingCapacity = daySummary.SessionSummaries.Sum(x => x.RemainingCapacity);
            
            var bookingsOnDay = bookings.Where(x => DateOnly.FromDateTime(x.From) == daySummary.Date).ToList();
            
            //includes both supported and orphaned bookings
            daySummary.TotalBooked = bookingsOnDay.Count(x => x.Status == AppointmentStatus.Booked);
            
            //...of which X are orphaned
            daySummary.TotalOrphaned = bookingsOnDay.Count(x =>
                x.Status == AppointmentStatus.Booked &&
                x.AvailabilityStatus == AvailabilityStatus.Orphaned);
            
            daySummary.TotalCancelled = bookingsOnDay.Count(x => x.Status == AppointmentStatus.Cancelled);
        }

        return new WeekSummary(daySummaries)
        {
            DaySummaries = daySummaries,
            MaximumCapacity = daySummaries.Sum(x => x.MaximumCapacity),
            RemainingCapacity = daySummaries.Sum(x => x.RemainingCapacity),
            TotalBooked = daySummaries.Sum(x => x.TotalBooked),
            TotalCancelled = daySummaries.Sum(x => x.TotalCancelled),
            TotalOrphaned = daySummaries.Sum(x => x.TotalOrphaned)
        };
    }

    private List<DaySummary> InitialiseDaySummaries(DateTime from, DateTime to, List<SessionInstance> sessions)
    {
        var dayDate = DateOnly.FromDateTime(from.Date);
            
        List<DaySummary> daySummaries = [
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
        //TODO any reason why we update cancelled Status when no longer supported? does it matter??
        if (booking.AvailabilityStatus is AvailabilityStatus.Supported &&
            booking.Status is not AppointmentStatus.Provisional)
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

    public List<SessionInstance> AvailableSlots { get; set; } = [];

    public WeekSummary WeekSummary { get; set; }
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
    ///     Return a summary of booking/availability for a week period
    /// </summary>
    WeekSummary = 2
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
