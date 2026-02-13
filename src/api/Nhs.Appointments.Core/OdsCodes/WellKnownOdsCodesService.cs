namespace Nhs.Appointments.Core.OdsCodes;

public class WellKnownOdsCodesService(IWellKnownOdsCodesStore wellKnownOdsCodesStore) : IWellKnowOdsCodesService
{
    public async Task<IEnumerable<WellKnownOdsEntry>> GetWellKnownOdsCodeEntries()
    {
        return await wellKnownOdsCodesStore.GetWellKnownOdsCodesDocument();
    }
}
