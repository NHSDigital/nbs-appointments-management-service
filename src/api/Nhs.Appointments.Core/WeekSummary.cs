namespace Nhs.Appointments.Core
{
    public class Summary : AvailabilityMetrics
    {
        public IEnumerable<DaySummary> DaySummaries { get; init; }
        
        // MaximumCapacity = DaySummaries.Sum(x => x.MaximumCapacity),
        // RemainingCapacity = DaySummaries.Sum(x => x.RemainingCapacity),
        // TotalSupportedAppointments = DaySummaries.Sum(x => x.TotalSupportedAppointments)
    }
    
    public class DaySummary(DateOnly date, IEnumerable<SessionSummary> sessions) : AvailabilityMetrics
    {
        public DateOnly Date { get; set; } = date;
        public readonly IEnumerable<SessionSummary> Sessions = sessions;
        public int TotalCancelledAppointments { get; set; }
    }
    
    public class SessionSummary
    {
        public Guid Id { get; init; }
        
        public DateTime UkStartDatetime { get; set; }
        
        public DateTime UkEndDatetime { get; set; }
        
        public Dictionary<string, int> TotalSupportedAppointmentsByService { get; init; }
        
        public int Capacity { get; init; }
        
        public int SlotLength { get; init; }
        
        public int MaximumCapacity { get; init; }
        
        public int TotalSupportedAppointments => TotalSupportedAppointmentsByService.Sum(x => x.Value);
        
        public int RemainingCapacity => MaximumCapacity - TotalSupportedAppointments;
    }

    public class AvailabilityMetrics
    {
        public int MaximumCapacity { get; set; }
        public int RemainingCapacity { get; set; }
        public int TotalSupportedAppointments { get; set; }
        public int TotalOrphanedAppointments => TotalOrphanedAppointmentsByService.Sum(x => x.Value);
        public Dictionary<string, int> TotalOrphanedAppointmentsByService { get; init; }
    }
}

