namespace Nhs.Appointments.Core;

public class WellKnownOdsCodesService(IWellKnownOdsCodesStore wellKnownOdsCodesStore) : IWellKnowOdsCodesService
{
    public Task<IEnumerable<WellKnownOdsEntry>> GetWellKnownOdsCodeEntries()
    {
        return wellKnownOdsCodesStore.GetWellKnownOdsCodesDocument();
    }
}
