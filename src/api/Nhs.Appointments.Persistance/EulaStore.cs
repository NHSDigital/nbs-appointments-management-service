using Nhs.Appointments.Core.Eula;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class EulaStore(ITypedDocumentCosmosStore<EulaDocument> documentStore) : IEulaStore
{
    public async Task<EulaVersion> GetLatestVersion()
    {
        var eulaDocument = await documentStore.GetDocument("eula");
        return new EulaVersion
        {
            VersionDate = eulaDocument.VersionDate
        };
    }
}
