namespace Nhs.Appointments.Core;

public interface IAvailabilityCalculator
{
    Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from, DateOnly until);
}

public class AvailabilityCalculator : IAvailabilityCalculator
{
    private readonly IScheduleService _scheduleService;
    private readonly IBookingsDocumentStore _bookingDocumentStore;

    public AvailabilityCalculator(IScheduleService scheduleService, IBookingsDocumentStore bookingsDocumentStore)
    {
        _scheduleService = scheduleService;
        _bookingDocumentStore = bookingsDocumentStore;
    }

    public async Task<IEnumerable<SessionInstance>> CalculateAvailability(string site, string service, DateOnly from, DateOnly until)
    {
        var sessions = await _scheduleService.GetSessions(site, service, from, until);
        if (sessions.Any())
        {
            var blockDict = sessions.GroupBy(s => s.SessionHolder).ToDictionary(g => g.Key, g => g.ToList());

            var bookings = await _bookingDocumentStore.GetInDateRangeAsync(from.ToDateTime(new TimeOnly(0,0)) , until.ToDateTime(new TimeOnly(23,59)), site);
            var isNotCancelled = (Booking b) => b.Outcome?.ToLower() != "cancelled";
            var liveBookings = bookings.Where(isNotCancelled);
        
            foreach (var booking in liveBookings)
            {
                var blocks = blockDict[booking.SessionHolder];
                var targetBlock = blocks.Single(block => block.Contains(booking.TimePeriod));
                var newBlocks = targetBlock.Split(booking.TimePeriod).Select(b => new SessionInstance(b){SessionHolder = booking.SessionHolder});
                blocks.Remove(targetBlock);
                blocks.AddRange(newBlocks);
            }
            return blockDict.SelectMany(kvp => kvp.Value);
        }
        return Enumerable.Empty<SessionInstance>();
    }
}
