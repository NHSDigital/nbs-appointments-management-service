using AutoMapper;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class ClinicalServiceStore(ITypedDocumentCosmosStore<ClinicalServiceDocument> documentStore, IMapper mapper) : IClinicalServiceStore
{
    private const string ClinicalServicesDocumentId = "clinical_services";
    public async Task<IEnumerable<ClinicalServiceType>> Get()
    {
        var clinicalServiceDocument = await documentStore.GetByIdAsync<ClinicalServiceDocument>(ClinicalServicesDocumentId);

        return clinicalServiceDocument.Services.Select(mapper.Map<ClinicalServiceType>);
    }
}
