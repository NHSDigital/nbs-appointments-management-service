namespace Nhs.Appointments.Core;

public interface IAvailabilityStateService
{
    Task<AvailabilityState> Build(string site, DateTime from, DateTime to, string service, bool processRecalculations = true);
}
