using Microsoft.Extensions.Options;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

internal class ReferenceGroupCosmosDocumentStore : IReferenceNumberWriteStore
{
    private const string DocumentId = "main";
    private readonly ITypedDocumentCosmosStore<ReferenceGroupDocument> _cosmosStore;
    private readonly ReferenceGroupOptions _options;
    private ReferenceGroupDocument document;

    public ReferenceGroupCosmosDocumentStore(ITypedDocumentCosmosStore<ReferenceGroupDocument> cosmosStore, IOptions<ReferenceGroupOptions> options)
    {
        _cosmosStore = cosmosStore;
        _options = options.Value;
        document = new ReferenceGroupDocument
        {
            DocumentType = _cosmosStore.GetDocumentType(),
            Id = DocumentId,
            Groups = Enumerable.Range(0, _options.InitialGroupCount).Select(x => new ReferenceGroup
            {
                Prefix = x,
                Sequence = 0,
                SiteCount = 0
            }).ToArray()
        };
    }

    public async Task<int> AssignReferenceGroup()
    {
        var target = document!.Groups.Where(g => g.Prefix > 0)
            .OrderBy(g => g.SiteCount).ThenBy(g => g.Prefix).First();
        target.SiteCount++;

        return target.Prefix;
    }

    public async Task<int> GetNextSequenceNumber(int prefix)
    {
        var target = document!.Groups.Single(g => g.Prefix == prefix);
        target.Sequence++;
        return target.Sequence;
    }

    public async Task SaveReferenceGroup() => await _cosmosStore.WriteAsync(document);
}
