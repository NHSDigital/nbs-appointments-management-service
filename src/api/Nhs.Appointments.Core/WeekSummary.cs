namespace Nhs.Appointments.Core
{
    public class Summary : AvailabilityMetrics
    {
        public IEnumerable<DaySummary> DaySummaries { get; init; }
        public Dictionary<string, int> Orphaned { get; init; }
    }
    
    public class DaySummary(DateOnly date, IEnumerable<SessionSummary> sessions) : AvailabilityMetrics
    {
        public DateOnly Date { get; set; } = date;
        public readonly IEnumerable<SessionSummary> Sessions = sessions;
        public int CancelledAppointments { get; set; }
    }
    
    public class SessionSummary
    {
        public Guid Id { get; init; }
        
        public DateTime UkStartDatetime { get; set; }
        
        public DateTime UkEndDatetime { get; set; }
        public Dictionary<string, int> Bookings { get; init; }
        
        public int Capacity { get; init; }
        
        public int SlotLength { get; init; }
        
        public int MaximumCapacity { get; init; }
        
        public int TotalBookings => Bookings.Sum(x => x.Value);
        
        public int RemainingCapacity => MaximumCapacity - TotalBookings;
    }

    public class AvailabilityMetrics
    {
        public int MaximumCapacity { get; set; }
        public int RemainingCapacity { get; set; }
        public int BookedAppointments { get; set; }
        public int OrphanedAppointments { get; set; }
    }
}

