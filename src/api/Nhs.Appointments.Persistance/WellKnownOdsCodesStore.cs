using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class WellKnownOdsCodesStore(ITypedDocumentCosmosStore<WellKnownOdsCodesDocument> cosmosStore) : IWellKnownOdsCodesStore
{
    private const string WellKnownOdsCodeDocumentId = "well_known_ods_codes";
    public async Task<IEnumerable<WellKnownOdsEntry>> GetWellKnownOdsCodesDocument()
    {
        var document = await cosmosStore.GetDocument<WellKnownOdsCodesDocument>(WellKnownOdsCodeDocumentId);
        return document.Entries;
    }
}
