using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance
{
    public class ReferenceGroupCosmosDocumentStore : IReferenceNumberDocumentStore
    {
        private const string DocumentId = "main";
        private readonly ITypedDocumentCosmosStore<ReferenceGroupDocument> _cosmosStore;
        private readonly ReferenceGroupOptions _options;

        public ReferenceGroupCosmosDocumentStore(ITypedDocumentCosmosStore<ReferenceGroupDocument> cosmosStore, IOptions<ReferenceGroupOptions> options)
        {
            _cosmosStore = cosmosStore;
            _options = options.Value;
        }

        public async Task<int> AssignReferenceGroup()
        {
            ReferenceGroupDocument referenceGroupDocument;
            var docType = _cosmosStore.GetDocumentType();
            try
            {
                referenceGroupDocument = await _cosmosStore.GetByIdAsync<ReferenceGroupDocument>("main");
            }
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                referenceGroupDocument = new ReferenceGroupDocument
                {
                    DocumentType = docType,
                    Id = DocumentId,
                    Groups = Enumerable.Range(0, _options.InitialGroupCount).Select(x => new ReferenceGroup
                    {
                        Prefix = x,
                        Sequence = 0,
                        SiteCount = 0
                    }).ToArray()
                };

                await _cosmosStore.WriteAsync(referenceGroupDocument);
            }
            
            var target = referenceGroupDocument!.Groups.Where(g => g.Prefix > 0)
                .OrderBy(g => g.SiteCount).ThenBy(g => g.Prefix).First();
            var siteCountIncrement = PatchOperation.Increment($"/Groups/{target.Prefix}/SiteCount", 1);
            
            await _cosmosStore.PatchDocument(docType, DocumentId, siteCountIncrement);
            return target.Prefix;
        }

        public async Task<int> GetNextSequenceNumber(int prefix)
        {
            var incrementSequencePatch = PatchOperation.Increment($"/Groups/{prefix}/Sequence", 1);
            var docType = _cosmosStore.GetDocumentType();
            var referenceGroupDocument = await _cosmosStore.PatchDocument(docType, DocumentId, incrementSequencePatch);
            return referenceGroupDocument.Groups.Single(gr => gr.Prefix == prefix).Sequence;
        }
    }

    public class ReferenceGroupOptions
    {
        public int InitialGroupCount { get; set; }
    }
}
