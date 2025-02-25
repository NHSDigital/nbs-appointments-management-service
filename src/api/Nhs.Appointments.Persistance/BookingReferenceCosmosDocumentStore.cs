using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance
{
    public class BookingReferenceCosmosDocumentStore(ITypedDocumentCosmosStore<BookingReferenceDocument> cosmosStore)
        : IBookingReferenceDocumentStore
    {
        private const string DocumentId = "reference_number";

        public async Task<int> GetNextSequenceNumber()
        {
            var incrementSequencePatch = PatchOperation.Increment("/sequence", 1);
            var docType = cosmosStore.GetDocumentType();
            var referenceGroupDocument = await cosmosStore.PatchDocument(docType, DocumentId, incrementSequencePatch);
            return referenceGroupDocument.Sequence;
        }
    }
}
