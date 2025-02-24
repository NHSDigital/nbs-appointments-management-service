using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance
{
    public class ReferenceGroupCosmosDocumentStore : IReferenceNumberDocumentStore
    {
        private const string DocumentId = "reference_number";
        private readonly ITypedDocumentCosmosStore<ReferenceGroupDocument> _cosmosStore;

        public ReferenceGroupCosmosDocumentStore(ITypedDocumentCosmosStore<ReferenceGroupDocument> cosmosStore)
        {
            _cosmosStore = cosmosStore;
        }

        public async Task<int> GetNextSequenceNumber()
        {
            var incrementSequencePatch = PatchOperation.Increment("/sequence", 1);
            var docType = _cosmosStore.GetDocumentType();
            var referenceGroupDocument = await _cosmosStore.PatchDocument(docType, DocumentId, incrementSequencePatch);
            return referenceGroupDocument.Sequence;
        }
    }
}
