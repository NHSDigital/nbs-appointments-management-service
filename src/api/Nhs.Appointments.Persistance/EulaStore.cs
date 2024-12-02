using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class EulaStore(ITypedDocumentCosmosStore<EulaDocument> documentStore) : IEulaStore
{
    public async Task<EulaVersion> GetLatestVersion()
    {
        return await documentStore.GetDocument<EulaVersion>("eula");
    }
}