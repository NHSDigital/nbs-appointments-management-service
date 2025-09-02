namespace Nhs.Appointments.Core
{
    public class AvailabilitySummary : AvailabilityMetrics
    {
        public IEnumerable<DayAvailabilitySummary> DaySummaries { get; init; }

        public override Dictionary<string, int> TotalSupportedAppointmentsByService => DaySummaries.SelectMany(x => x.TotalSupportedAppointmentsByService)
            .GroupBy(kv => kv.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(kv => kv.Value)
            );
        
        public override int TotalOrphanedAppointments => DaySummaries.Sum(x => x.TotalOrphanedAppointments);

        public override int TotalSupportedAppointments => DaySummaries.Sum(x => x.TotalSupportedAppointments);
    }

    public class DayAvailabilitySummary : AvailabilityMetrics
    {
        public DayAvailabilitySummary(DateOnly date, IEnumerable<SessionAvailabilitySummary> sessionSummaries)
        {
            TotalOrphanedAppointmentsByService = new Dictionary<string, int>();

            Date = date;
            SessionSummaries = sessionSummaries;
        }

        public DateOnly Date { get; private set; }

        public IEnumerable<SessionAvailabilitySummary> SessionSummaries { get; }

        public int TotalCancelledAppointments { get; set; }

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
        public Dictionary<string, int> TotalOrphanedAppointmentsByService { get; init; }
    }
}
