namespace Nhs.Appointments.Core;

public interface IAllocationStateService
{
    Task<AllocationState> Build(string site, DateTime from, DateTime to, bool processRecalculations = true);
}
