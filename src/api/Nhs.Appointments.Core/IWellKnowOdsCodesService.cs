namespace Nhs.Appointments.Core;

public interface IWellKnowOdsCodesService
{
    public Task<IEnumerable<WellKnownOdsEntry>> GetWellKnownOdsCodeEntries();
}
