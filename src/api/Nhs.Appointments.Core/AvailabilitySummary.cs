using YamlDotNet.Serialization.NodeTypeResolvers;

namespace Nhs.Appointments.Core
{
    public class AvailabilitySummary(IEnumerable<DayAvailabilitySummary> daySummaries) : AvailabilityMetrics
    {
        public IEnumerable<DayAvailabilitySummary> DaySummaries { get; init; } = daySummaries;

        public override Dictionary<string, int> TotalSupportedAppointmentsByService => DaySummaries
            .SelectMany(x => x.TotalSupportedAppointmentsByService)
            .GroupBy(kv => kv.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(kv => kv.Value)
            );

        public override Dictionary<string, int> TotalRemainingCapacityByService => DaySummaries
            .Select(x => x.TotalRemainingCapacityByService)
            .SelectMany(dict => dict)
            .GroupBy(kv => kv.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(kv => kv.Value)
            );

        public new int MaximumCapacity => DaySummaries.Sum(x => x.MaximumCapacity);

        public override int TotalRemainingCapacity => DaySummaries.Sum(x => x.TotalRemainingCapacity);

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

        public override Dictionary<string, int> TotalRemainingCapacityByService => SessionSummaries
            .Select(x => x.TotalRemainingCapacityByService)
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

        public override int TotalRemainingCapacity => SessionSummaries.Sum(x => x.TotalRemainingCapacity);
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

        public int TotalSupportedAppointments => TotalSupportedAppointmentsByService.Sum(x => x.Value);

        public int TotalRemainingCapacity => MaximumCapacity - TotalSupportedAppointments;

        /// <summary>
        ///     This metric doesn't make much sense as capacity can be counted twice if a session supports multiple services
        ///     i.e Summing all of these does NOT equal TotalRemainingCapacity
        /// </summary>
        public Dictionary<string, int> TotalRemainingCapacityByService => TotalSupportedAppointmentsByService.ToDictionary(g => g.Key, g => MaximumCapacity - g.Value);
    }

    public abstract class AvailabilityMetrics
    {
        public int MaximumCapacity { get; set; }

        public abstract int TotalRemainingCapacity { get; }

        public abstract Dictionary<string, int> TotalRemainingCapacityByService { get; }

        public abstract int TotalSupportedAppointments { get; }

        public abstract Dictionary<string, int> TotalSupportedAppointmentsByService { get; }
        public abstract int TotalOrphanedAppointments { get; }
        public abstract Dictionary<string, int> TotalOrphanedAppointmentsByService { get; }

        public abstract int TotalCancelledAppointments { get; }
        public abstract Dictionary<string, int> TotalCancelledAppointmentsByService { get; }
    }
}
