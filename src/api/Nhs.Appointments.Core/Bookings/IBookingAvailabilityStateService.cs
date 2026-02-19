using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Core.Bookings;

public interface IBookingAvailabilityStateService
{
    Task<IEnumerable<SessionInstance>> GetAvailableSlots(string site, DateTime from, DateTime to);
    Task<AvailabilitySummary> GetWeekSummary(string site, DateOnly from);
    Task<AvailabilitySummary> GetDaySummary(string site, DateOnly day);
    Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from, DateTime to, NewlyUnsupportedBookingAction newlyUnsupportedBookingAction);
    Task<AvailabilityUpdateProposal> GenerateSessionProposalActionMetrics(string site, DateTime from, DateTime to, Session matcher, Session replacement);
    Task<(int SessionCount, int BookingCount)> GenerateCancelDateRangeProposalActionMetricsAsync(string site, DateOnly from, DateOnly to);
}
