namespace Nhs.Appointments.Core.OdsCodes;

public interface IWellKnowOdsCodesService
{
    public Task<IEnumerable<WellKnownOdsEntry>> GetWellKnownOdsCodeEntries();
}
