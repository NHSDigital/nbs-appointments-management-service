namespace Nhs.Appointments.Core
{
    public class AvailabilitySummary(IEnumerable<DayAvailabilitySummary> daySummaries) : AvailabilityMetrics
    {
        public IEnumerable<DayAvailabilitySummary> DaySummaries { get; init; } = daySummaries;

        public override Dictionary<string, int> TotalSupportedAppointmentsByService => DaySummaries.SelectMany(x => x.TotalSupportedAppointmentsByService)
            .GroupBy(kv => kv.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(kv => kv.Value)
            );
        
        public new int MaximumCapacity => DaySummaries.Sum(x => x.MaximumCapacity);
        
        public new int RemainingCapacity => DaySummaries.Sum(x => x.RemainingCapacity);
        
        public override int TotalOrphanedAppointments => DaySummaries.Sum(x => x.TotalOrphanedAppointments);
        public override Dictionary<string, int> TotalOrphanedAppointmentsByService => DaySummaries
            .Select(x => x.TotalOrphanedAppointmentsByService)
            .SelectMany(dict => dict)
            .GroupBy(kv => kv.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(kv => kv.Value)
            );

        public override int TotalSupportedAppointments => DaySummaries.Sum(x => x.TotalSupportedAppointments);

        public override Dictionary<string, int> TotalCancelledAppointmentsByService => DaySummaries
            .Select(x => x.TotalCancelledAppointmentsByService)
            .SelectMany(dict => dict)
            .GroupBy(kv => kv.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(kv => kv.Value)
            );
        
        public override int TotalCancelledAppointments => TotalCancelledAppointmentsByService.Sum(x => x.Value);
    }

    public class DayAvailabilitySummary(DateOnly date, IEnumerable<SessionAvailabilitySummary> sessionSummaries)
        : AvailabilityMetrics
    {
        public DateOnly Date { get; private set; } = date;

        public IEnumerable<SessionAvailabilitySummary> SessionSummaries { get; } = sessionSummaries;

        public override int TotalCancelledAppointments => TotalCancelledAppointmentsByService.Sum(x => x.Value);

        public override Dictionary<string, int> TotalSupportedAppointmentsByService => SessionSummaries
            .Select(x => x.TotalSupportedAppointmentsByService)
            .SelectMany(dict => dict)
            .GroupBy(kv => kv.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(kv => kv.Value)
            );

        public override int TotalSupportedAppointments => TotalSupportedAppointmentsByService.Sum(x => x.Value);

        public override int TotalOrphanedAppointments => TotalOrphanedAppointmentsByService.Sum(x => x.Value);
        public override Dictionary<string, int> TotalOrphanedAppointmentsByService { get; } = new();

        public override Dictionary<string, int> TotalCancelledAppointmentsByService { get; } = new();

        public new int MaximumCapacity => SessionSummaries.Sum(x => x.MaximumCapacity);
        
        public new int RemainingCapacity => SessionSummaries.Sum(x => x.RemainingCapacity);
    }

    public class SessionAvailabilitySummary
    {
        public Guid Id { get; init; }

        public DateTime UkStartDatetime { get; set; }

        public DateTime UkEndDatetime { get; set; }

        public Dictionary<string, int> TotalSupportedAppointmentsByService { get; init; }

        public int Capacity { get; init; }

        public int SlotLength { get; init; }

        public int MaximumCapacity { get; init; }

        private int TotalSupportedAppointments => TotalSupportedAppointmentsByService.Sum(x => x.Value);

        public int RemainingCapacity => MaximumCapacity - TotalSupportedAppointments;
    }

    public abstract class AvailabilityMetrics
    {
        public int MaximumCapacity { get; set; }
        public int RemainingCapacity { get; set; }
        public abstract int TotalSupportedAppointments { get; }

        public abstract Dictionary<string, int> TotalSupportedAppointmentsByService { get; }
        public abstract int TotalOrphanedAppointments { get; }
        public abstract Dictionary<string, int> TotalOrphanedAppointmentsByService { get; }
        
        public abstract int TotalCancelledAppointments { get; }
        public abstract Dictionary<string, int> TotalCancelledAppointmentsByService { get; }
    }
}
