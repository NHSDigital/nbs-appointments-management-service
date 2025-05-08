namespace Nhs.Appointments.Core;

public interface IAllocationStateService
{
    Task<AllocationState> BuildAllocation(string site, DateTime from, DateTime to);
    Task<IEnumerable<BookingAvailabilityUpdate>> BuildRecalculations(string site, DateTime from, DateTime to);
}
