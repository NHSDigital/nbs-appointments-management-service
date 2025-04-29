namespace Nhs.Appointments.Core;

public interface IAllocationStateService
{
    Task<AllocationState> Build(string site, DateTime from, DateTime to, string service, bool processRecalculations = true);
}
