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
            BookingReferenceDocument bookingReferenceDocument;
            
            try
            {
                var incrementSequencePatch = PatchOperation.Increment("/sequence", 1);
                var docType = cosmosStore.GetDocumentType();
                bookingReferenceDocument = await cosmosStore.PatchDocument(docType, DocumentId, incrementSequencePatch);
            }
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                bookingReferenceDocument = new BookingReferenceDocument
                {
                    DocumentType = cosmosStore.GetDocumentType(),
                    Id = DocumentId,
                    Sequence = 0
                };

                await cosmosStore.WriteAsync(bookingReferenceDocument);
            }
            
            return bookingReferenceDocument.Sequence;
        }
    }
}
