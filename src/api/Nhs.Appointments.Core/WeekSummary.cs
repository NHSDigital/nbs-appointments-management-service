namespace Nhs.Appointments.Core
{
    public class WeekSummary(IEnumerable<DaySummary> daySummaries) : AvailabilityMetrics
    {
        public IEnumerable<DaySummary> DaySummaries { get; set; }
    }
    
    public class DaySummary(DateOnly date, IEnumerable<SessionSummary> sessionSummaries) : AvailabilityMetrics
    {
        public DateOnly Date { get; set; } = date;
        public readonly IEnumerable<SessionSummary> SessionSummaries = sessionSummaries;
    }
    
    public class SessionSummary
    {
        public Guid Id { get; init; }
        public DateTime From { get; set; }
        public DateTime Until { get; set; }
        public Dictionary<string, int> ServiceBookings { get; init; }
        
        public int MaximumCapacity { get; init; }
        
        public int TotalBooked => ServiceBookings.Sum(x => x.Value);
        
        public int RemainingCapacity => MaximumCapacity - TotalBooked;
    }

    public class AvailabilityMetrics
    {
        public int MaximumCapacity { get; set; }
        public int RemainingCapacity { get; set; }
        public int TotalBooked { get; set; }
        public int TotalCancelled { get; set; }
        public int TotalOrphaned { get; set; }
    }
}

