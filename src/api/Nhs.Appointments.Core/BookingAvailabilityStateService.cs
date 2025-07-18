namespace Nhs.Appointments.Core;

public class BookingAvailabilityStateService(
    IAvailabilityQueryService availabilityQueryService,
    IBookingQueryService bookingQueryService) : IBookingAvailabilityStateService
{
    private readonly IReadOnlyList<AppointmentStatus> _liveStatuses =
        [AppointmentStatus.Booked, AppointmentStatus.Provisional];

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

    public async Task<IEnumerable<SessionInstance>> GetAvailableSlots(string site, DateTime from, DateTime to)
    {
        return (await BuildState(site, from, to, BookingAvailabilityStateReturnType.AvailableSlots)).AvailableSlots;
    }

    private async Task<(IEnumerable<Booking> bookings, List<LinkedSessionInstance> sessions)> FetchData(string site,
        DateTime from, DateTime to, BookingAvailabilityStateReturnType returnType)
    {
        var isWeekSummary = returnType == BookingAvailabilityStateReturnType.WeekSummary;

        var statuses = isWeekSummary ? [AppointmentStatus.Booked, AppointmentStatus.Cancelled] : _liveStatuses;
        var bookingsTask = bookingQueryService.GetOrderedBookings(site, from, to, statuses);
        var sessionsTask = availabilityQueryService.GetLinkedSessions(site, DateOnly.FromDateTime(from),
            DateOnly.FromDateTime(to), isWeekSummary);
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
                case BookingAvailabilityStateReturnType.WeekSummary:
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

    private static WeekSummary GenerateWeekSummary(IEnumerable<Booking> bookings, List<DaySummary> daySummaries)
    {
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

        return new WeekSummary
        {
            DaySummaries = daySummaries,
            MaximumCapacity = daySummaries.Sum(x => x.MaximumCapacity),
            RemainingCapacity = daySummaries.Sum(x => x.RemainingCapacity),
            BookedAppointments = daySummaries.Sum(x => x.BookedAppointments),
            OrphanedAppointments = daySummaries.Sum(x => x.OrphanedAppointments)
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
