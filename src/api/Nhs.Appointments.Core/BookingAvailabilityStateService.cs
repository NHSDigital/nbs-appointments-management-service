namespace Nhs.Appointments.Core;

public class BookingAvailabilityStateService(
    IAvailabilityQueryService availabilityQueryService,
    IBookingQueryService bookingQueryService) : IBookingAvailabilityStateService
{
    private readonly IReadOnlyList<AppointmentStatus> _liveStatuses =
        [AppointmentStatus.Booked, AppointmentStatus.Provisional];

    public async Task<AvailabilitySummary> GetWeekSummary(string site, DateOnly from)
    {
        var dayStart = from.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = from.AddDays(6).ToDateTime(new TimeOnly(23, 59, 59));

        return (await BuildState(site, dayStart, dayEnd, BookingAvailabilityStateReturnType.Summary))
            .Summary;
    }

    public async Task<AvailabilitySummary> GetDaySummary(string site, DateOnly day)
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

    public async Task<AvailabilityUpdateProposal> BuildRecalculations(string site, DateTime from,
    DateTime to, Session matcher, Session replacement, bool isWildCard)
    {
        return (await BuildState(
            site, 
            from, 
            to, 
            BookingAvailabilityStateReturnType.Recalculations, 
            matcher, 
            replacement,
            isWildCard))
            .UpdateProposal;
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

    private static IEnumerable<SessionAvailabilitySummary> InitialiseDailySessionSummaries(DateOnly date,
        List<LinkedSessionInstance> sessionInstances)
    {
        return sessionInstances.Where(x => DateOnly.FromDateTime(x.From).Equals(date)).Select(x => new SessionAvailabilitySummary
        {
            Id = x.InternalSessionId!.Value,
            UkStartDatetime = x.From,
            UkEndDatetime = x.Until,
            MaximumCapacity = x.Capacity * x.ToSlots().Count(),
            Capacity = x.Capacity,
            SlotLength = x.SlotLength,
            TotalSupportedAppointmentsByService = x.Services.ToDictionary(key => key, _ => 0)
        }).ToList();
    }

    private BookingAvailabilityState BuildState(IEnumerable<Booking> bookings, List<LinkedSessionInstance> sessions,
        BookingAvailabilityStateReturnType returnType, DateTime from, DateTime to)
    {
        var state = new BookingAvailabilityState();

        //have to materialise to a list as we transform the data within
        var slotsList = sessions.SelectMany(session => session.ToSlots()).ToList();

        var liveBookings = bookings.Where(x => _liveStatuses.Contains(x.Status));

        List<DayAvailabilitySummary> daySummaries = [];

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
                        state.UpdateProposal.SupportedBookingsCount++;
                        break;
                    case BookingAvailabilityStateReturnType.Summary:
                        //update status if not already supported

                        if (booking.AvailabilityStatus != AvailabilityStatus.Supported)
                        {
                            booking.AvailabilityStatus = AvailabilityStatus.Supported;
                        }

                        var fromDate = DateOnly.FromDateTime(targetSlot.From);
                        var sessionToUpdate = daySummaries.Where(x => x.Date == fromDate)
                            .SelectMany(x => x.SessionSummaries)
                            .Single(x => x.Id == targetSlot.InternalSessionId);

                        sessionToUpdate.TotalSupportedAppointmentsByService[booking.Service]++;
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
                    if (booking.Status is AppointmentStatus.Booked)
                    {
                        if (booking.AvailabilityStatus is AvailabilityStatus.Supported)
                        {
                            booking.AvailabilityStatus = AvailabilityStatus.Orphaned;
                        }
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
                state.UpdateProposal.UnsupportedBookingsCount = liveBookings.Count() - state.UpdateProposal.SupportedBookingsCount;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(returnType), returnType, null);
        }

        return state;
    }

    private async Task<BookingAvailabilityState> BuildState(
        string site, 
        DateTime from, 
        DateTime to,
        BookingAvailabilityStateReturnType returnType, 
        Session matcher = null, 
        Session replacement = null,
        bool? isWildCard = null)
    {
        var (bookings, sessions) =
                await FetchData(site, from, to, returnType);

        var proposalAction = DetermineAvailabilityUpdateProposalAction(isWildcard: isWildCard ?? false, (matcher != null && replacement != null));

        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
        {
            var sessionsForDay = sessions.Where(s => s.From.Date == day).ToList();

            switch (proposalAction)
            {
                case AvailabilityUpdateProposalAction.CancelAll:
                    foreach (var s in sessionsForDay)
                        sessions.Remove(s);
                    break;

                case AvailabilityUpdateProposalAction.CancelSingle:
                    if (matcher is null)
                        break;

                    var matchedSession = sessionsForDay.FindMatchingSession(matcher);

                    if (matchedSession is null)
                        return new BookingAvailabilityState(matchingSessionNotFound: true);

                    sessions.Remove(matchedSession);
                    break;

                case AvailabilityUpdateProposalAction.Edit:
                    if (matcher is null || replacement is null)
                        break;

                    var matched = sessionsForDay.FindMatchingSession(matcher);

                    if (matched is null)
                        return new BookingAvailabilityState(matchingSessionNotFound: true);

                    sessions.Remove(matched);

                    var replacementSession = new LinkedSessionInstance(
                        day.Add(replacement.From.ToTimeSpan()),
                        day.Add(replacement.Until.ToTimeSpan())
                    )
                    {
                        Services = replacement.Services,
                        Capacity = replacement.Capacity,
                        SlotLength = replacement.SlotLength
                    };

                    sessions.Add(replacementSession);
                    break;
            }
        }
        
        return BuildState(bookings, sessions, returnType, from, to);
    }



    private static AvailabilityUpdateProposalAction DetermineAvailabilityUpdateProposalAction(bool isWildcard, bool hasMatcherAndReplacementSessions)
    {
        if (hasMatcherAndReplacementSessions)
        {
            return AvailabilityUpdateProposalAction.Edit;
        }

        if (isWildcard)
        {
            return AvailabilityUpdateProposalAction.CancelAll;
        }
        else {
            return AvailabilityUpdateProposalAction.CancelSingle;
        }
    }

    public enum AvailabilityUpdateProposalAction
    {
        CancelAll,
        CancelSingle,
        Edit,
    }
    private static AvailabilitySummary GenerateSummary(IEnumerable<Booking> bookings, List<DayAvailabilitySummary> daySummaries)
    {
        foreach (var daySummary in daySummaries)
        {
            var bookingsOnDay = bookings.Where(x => DateOnly.FromDateTime(x.From) == daySummary.Date).ToList();

            foreach (var booking in bookingsOnDay)
            {
                switch (booking.Status)
                {
                    case AppointmentStatus.Booked:
                        switch (booking.AvailabilityStatus)
                        {
                            case AvailabilityStatus.Supported:
                                daySummary.TotalSupportedAppointmentsByService[booking.Service] = daySummary.TotalSupportedAppointmentsByService.GetValueOrDefault(booking.Service, 0) + 1;
                                break;
                            case AvailabilityStatus.Orphaned:
                                daySummary.TotalOrphanedAppointmentsByService[booking.Service] = daySummary.TotalOrphanedAppointmentsByService.GetValueOrDefault(booking.Service, 0) + 1;
                                break;
                        }
            
                        break;
                    case AppointmentStatus.Cancelled:
                        daySummary.TotalCancelledAppointmentsByService[booking.Service] = daySummary.TotalCancelledAppointmentsByService.GetValueOrDefault(booking.Service, 0) + 1;
                        break;
                }
            }
        }

        return new AvailabilitySummary(daySummaries);
    }

    private static List<DayAvailabilitySummary> InitialiseDaySummaries(
        DateTime from, 
        DateTime to,
        List<LinkedSessionInstance> sessions)
    {
        var dayDate = DateOnly.FromDateTime(from.Date);

        List<DayAvailabilitySummary> daySummaries =
        [
            new(dayDate, InitialiseDailySessionSummaries(dayDate, sessions))
        ];

        while (dayDate < DateOnly.FromDateTime(to.Date))
        {
            dayDate = dayDate.AddDays(1);
            daySummaries.Add(new DayAvailabilitySummary(dayDate, InitialiseDailySessionSummaries(dayDate, sessions)));
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

    public static LinkedSessionInstance? FindMatchingSession(this List<LinkedSessionInstance> sessions, Session matcher)
    {
        return sessions.FirstOrDefault(s =>
                        s.From.TimeOfDay == matcher.From.ToTimeSpan() &&
                        s.Until.TimeOfDay == matcher.Until.ToTimeSpan() &&
                        s.Duration == (matcher.Until - matcher.From) &&
                        s.Capacity == matcher.Capacity &&
                        matcher.Services.All(ms => s.Services.Contains(ms))
                    );
    }
}

public class BookingAvailabilityState
{
    public BookingAvailabilityState() {}
    public BookingAvailabilityState(bool matchingSessionNotFound)
    {
        UpdateProposal = new AvailabilityUpdateProposal(matchingSessionNotFound);
    }
    public readonly List<BookingAvailabilityUpdate> BookingAvailabilityUpdates = [];

    public IEnumerable<SessionInstance> AvailableSlots { get; set; } = [];

    public AvailabilitySummary Summary { get; set; }

    public AvailabilityUpdateProposal UpdateProposal { get; set; } = new AvailabilityUpdateProposal();
}

public class AvailabilityUpdateProposal
{
    public AvailabilityUpdateProposal(){}
    public AvailabilityUpdateProposal(bool matchingSessionNotFound)
    {
        MatchingSessionNotFound = matchingSessionNotFound;
    }
    public int SupportedBookingsCount { get; set; }
    public int UnsupportedBookingsCount { get; set; } 
    public bool MatchingSessionNotFound { get; set; }
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
