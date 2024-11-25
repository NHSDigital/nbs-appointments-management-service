using Nhs.Appointments.Persistance.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance;

public class EulaStore(ITypedDocumentCosmosStore<EulaDocument> documentStore) : IEulaStore
{
    public async Task<EulaVersion> GetLatestVersion()
    {
        return await documentStore.GetDocument<EulaVersion>("eula");
    }
}