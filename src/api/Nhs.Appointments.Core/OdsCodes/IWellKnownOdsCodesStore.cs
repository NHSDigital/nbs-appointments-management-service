namespace Nhs.Appointments.Core.OdsCodes;

public interface IWellKnownOdsCodesStore
{
    public Task<IEnumerable<WellKnownOdsEntry>> GetWellKnownOdsCodesDocument();
}
