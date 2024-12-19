namespace Nhs.Appointments.Core;

public interface IWellKnownOdsCodesStore
{
    public Task<IEnumerable<WellKnownOdsEntry>> GetWellKnownOdsCodesDocument();
}
