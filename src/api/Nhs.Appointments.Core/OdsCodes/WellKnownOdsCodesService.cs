namespace Nhs.Appointments.Core.OdsCodes;

public class WellKnownOdsCodesService(IWellKnownOdsCodesStore wellKnownOdsCodesStore) : IWellKnowOdsCodesService
{
    public Task<IEnumerable<WellKnownOdsEntry>> GetWellKnownOdsCodeEntries()
    {
        return wellKnownOdsCodesStore.GetWellKnownOdsCodesDocument();
    }
}
