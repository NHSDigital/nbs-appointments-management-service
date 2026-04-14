using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Core.Bookings;

public class BookingAvailabilityStateService(
    IAvailabilityQueryService availabilityQueryService,
    IBookingQueryService bookingQueryService) : IBookingAvailabilityStateService
{
    private readonly IReadOnlyList<AppointmentStatus> _liveStatuses =
        [AppointmentStatus.Booked, AppointmentStatus.Provisional];

    public async Task<AvailabilitySummary> GetWeekSummary(string site, DateOnly from)
    {
        var instruction = BookingAvailabilityStateInstructionFactory.CreateWeekSummaryInstruction(from);

        return (await BuildFullState(site, instruction)).Summary;
    }

    public async Task<AvailabilitySummary> GetDaySummary(string site, DateOnly day)
    {
        var instruction = BookingAvailabilityStateInstructionFactory.CreateDaySummaryInstruction(day);

        return (await BuildFullState(site, instruction)).Summary;
    }
    
    public async Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from,
        DateTime to, NewlyUnsupportedBookingAction newlyUnsupportedBookingAction)
    {
        // TODO: Create the combination of from to and the return type as a BookingAvailabilityStateInstruction instance from a factory method.
        // e.g. BookingAvailabilityStateInstructionFactory.CreateRecalculation(from, to)
        // Pass this object into FetchData and BuildState, rather than the separate parameters.
        var instruction = BookingAvailabilityStateInstructionFactory.CreateRecalculationsInstruction(from, to);
        var (bookings, sessions) = await FetchData(site, instruction);

        return BuildState(bookings, sessions, instruction, newlyUnsupportedBookingAction).BookingAvailabilityUpdates;
    }

    public async Task<AvailabilityUpdateProposal> GenerateSessionProposalActionMetrics(string site, DateTime from,
    DateTime to, Session matcher, Session replacement)
    {
        var instruction = BookingAvailabilityStateInstructionFactory.CreateSessionUpdateProposalInstruction(from, to);

        var (bookings, sessions) = await FetchData(site, instruction);

        var proposalAction = DetermineAvailabilityUpdateProposalAction(matcher, replacement);

        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
        {
            var sessionsForDay = sessions.Where(s => s.From.Date == day).ToList();

            switch (proposalAction)
            {
                case AvailabilityUpdateProposalAction.Cancel:
                    ArgumentNullException.ThrowIfNull(matcher);

                    var matchedSession = sessionsForDay.FindMatchingSession(matcher);

                    if (matchedSession is null)
                    {
                        return new AvailabilityUpdateProposal(matchingSessionNotFound: true);
                    }

                    sessions.Remove(matchedSession);
                    break;

                case AvailabilityUpdateProposalAction.Edit:
                    if (matcher is null || replacement is null)
                    {
                        break;
                    }

                    var matched = sessionsForDay.FindMatchingSession(matcher);

                    if (matched is null)
                    {
                        return new AvailabilityUpdateProposal(matchingSessionNotFound: true);
                    }

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

        return BuildState(bookings, sessions, instruction).UpdateProposal;
    }

    public async Task<IEnumerable<SessionInstance>> GetAvailableSlots(string site, DateTime from, DateTime to)
    {
        var instruction = BookingAvailabilityStateInstructionFactory.CreateAvailableSlotsInstruction(from, to);

        return (await BuildFullState(site, instruction)).AvailableSlots;
    }

    public async Task<(int SessionCount, int BookingCount)> GenerateCancelDateRangeProposalActionMetricsAsync(string site, DateOnly from, DateOnly to)
    {
        var instruction = BookingAvailabilityStateInstructionFactory.CreateDateRangeSummaryInstruction(from, to);

        var (bookings, sessions) = await FetchData(site, instruction);

        return (sessions.Count, bookings.Count(b => b.AvailabilityStatus == AvailabilityStatus.Supported));
    }

    /// <summary>
    /// Fetches all required data and builds up the state
    /// </summary>
    private async Task<BookingAvailabilityState> BuildFullState(string site, BookingAvailabilityStateInstruction instruction)
    {
        var (bookings, sessions) = await FetchData(site, instruction);

        return BuildState(bookings, sessions, instruction);
    }

    private async Task<(IEnumerable<Booking> bookings, List<LinkedSessionInstance> sessions)> FetchData(string site,
        BookingAvailabilityStateInstruction instruction)
    {
        var isSummary = instruction.ReturnType == BookingAvailabilityStateReturnType.Summary;

        var statuses = isSummary ? [AppointmentStatus.Booked, AppointmentStatus.Cancelled] : _liveStatuses;
        var bookingsTask = bookingQueryService.GetOrderedBookings(site, instruction.From, instruction.To, statuses);
        var sessionsTask = availabilityQueryService.GetLinkedSessions(site, DateOnly.FromDateTime(instruction.From),
            DateOnly.FromDateTime(instruction.To), isSummary);
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

    // TODO: Receive a BookingAvailabilityStateInstruction instance, rather than the separate from, to and returnType fields.
    private BookingAvailabilityState BuildState(IEnumerable<Booking> bookings, List<LinkedSessionInstance> sessions,
        BookingAvailabilityStateInstruction instruction, NewlyUnsupportedBookingAction? newlyUnsupportedBookingAction = null)
    {
        if (instruction.ReturnType == BookingAvailabilityStateReturnType.Recalculations)
        {
            //newlyUnsupportedBookingAction is only required if this is a Recalculations generation
            ArgumentNullException.ThrowIfNull(newlyUnsupportedBookingAction);
        }
        
        var state = new BookingAvailabilityState();

        //have to materialise to a list as we transform the data within
        var slotsList = sessions.SelectMany(session => session.ToSlots()).ToList();

        var liveBookings = bookings.Where(x => _liveStatuses.Contains(x.Status));

        List<DayAvailabilitySummary> daySummaries = [];

        if (instruction.ReturnType == BookingAvailabilityStateReturnType.Summary)
        {
            daySummaries = InitialiseDaySummaries(instruction.From, instruction.To, sessions);
        }

        foreach (var booking in liveBookings)
        {
            var targetSlot = ChooseHighestPrioritySlot(slotsList, booking);
            var bookingIsSupportedByAvailability = targetSlot is not null;

            if (bookingIsSupportedByAvailability)
            {
                switch (instruction.ReturnType)
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
                            .SelectMany(x => x.SessionSummaries)
                            .Single(x => x.Id == targetSlot.InternalSessionId);

                        sessionToUpdate.TotalSupportedAppointmentsByService[booking.Service]++;
                        break;
                    case BookingAvailabilityStateReturnType.SessionUpdateProposalMetrics:
                        if (booking.AvailabilityStatus is not AvailabilityStatus.Supported)
                        {
                            state.UpdateProposal.NewlySupportedBookingsCount++;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(instruction.ReturnType), instruction.ReturnType, null);
                }

                targetSlot.Capacity--;
                continue;
            }

            switch (instruction.ReturnType)
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
                    state.BookingAvailabilityUpdates.AppendNewlyUnsupportedBookings(booking, newlyUnsupportedBookingAction!.Value);
                    state.BookingAvailabilityUpdates.AppendProvisionalBookingsToBeDeleted(booking);
                    break;
                case BookingAvailabilityStateReturnType.SessionUpdateProposalMetrics:
                    if (booking.AvailabilityStatus is AvailabilityStatus.Supported &&
                        booking.Status is AppointmentStatus.Booked)
                    {
                        state.UpdateProposal.NewlyUnsupportedBookingsCount++;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(instruction.ReturnType), instruction.ReturnType, null);
            }
        }

        //finalise data
        switch (instruction.ReturnType)
        {
            case BookingAvailabilityStateReturnType.AvailableSlots:
                //we only need to do this calculation if we want to return the available slots
                state.AvailableSlots = slotsList.Where(s => s.Capacity > 0);
                break;
            case BookingAvailabilityStateReturnType.Summary:
                state.Summary = GenerateSummary(bookings, daySummaries);
                break;
            case BookingAvailabilityStateReturnType.Recalculations:
            case BookingAvailabilityStateReturnType.SessionUpdateProposalMetrics:
                //no further action
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(instruction.ReturnType), instruction.ReturnType, null);
        }

        return state;
    }

    private static AvailabilityUpdateProposalAction DetermineAvailabilityUpdateProposalAction(Session matcher, Session replacement)
    {
        return matcher != null && replacement != null ? AvailabilityUpdateProposalAction.Edit : AvailabilityUpdateProposalAction.Cancel;
    }

    private enum AvailabilityUpdateProposalAction
    {
        Cancel,
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
                        if (booking.AvailabilityStatus is AvailabilityStatus.Orphaned){
                            daySummary.TotalOrphanedAppointmentsByService[booking.Service] = daySummary.TotalOrphanedAppointmentsByService.GetValueOrDefault(booking.Service, 0) + 1;
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
