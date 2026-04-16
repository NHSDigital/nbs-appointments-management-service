using Nhs.Appointments.Core.ClinicalServices;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class ClinicalServiceStore(ITypedDocumentCosmosStore<ClinicalServiceDocument> documentStore) : IClinicalServiceStore
{
    private const string ClinicalServicesDocumentId = "clinical_services";
    public async Task<IEnumerable<ClinicalServiceType>> Get()
    {
        var clinicalServiceDocument = await documentStore.GetByIdAsync(ClinicalServicesDocumentId);

        return clinicalServiceDocument.Services.Select(s => new ClinicalServiceType
        {
            Label = s.Label,
            ServiceType = s.ServiceType,
            Url = s.Url,
            Value = s.Id
        });
    }
}
