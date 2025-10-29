using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core.ReferenceNumber.V2;
using Nhs.Appointments.Persistance.Models.Reference;

namespace Nhs.Appointments.Persistance.Reference
{
    public class BookingReferenceCosmosDocumentStore(ITypedDocumentCosmosStore<BookingReferenceDocument> cosmosStore)
        : IBookingReferenceDocumentStore
    {
        private const string DocumentId = "booking_reference";

        public async Task<int> GetNextSequenceNumber()
        {
            var incrementSequencePatch = PatchOperation.Increment("/sequence", 1);
            var docType = cosmosStore.GetDocumentType();
            var referenceGroupDocument = await cosmosStore.PatchDocument(docType, DocumentId, incrementSequencePatch);
            return referenceGroupDocument.Sequence;
        }
    }
}
