using Azure.Storage.Blobs;
using DataExtract;
using Parquet;
using Parquet.Schema;
using Parquet.Serialization;

namespace JobRunner.Job.Notify;

public class BlobSendTracker(IAzureBlobStorage azureBlobStorage) : ISendTracker
{
    private const string ContainerName = "notify";
    private List<BlobTrackerModel> _tracker = new();

    private bool _changesPersisted = false;

    public async Task RefreshState(string file)
    {
        _tracker = new();

        var stream = await azureBlobStorage.GetBlob(ContainerName, file);
        if (stream is not null)
        {
            _tracker = (await ParquetSerializer.DeserializeAsync<BlobTrackerModel>(stream)).ToList();
        }

        _changesPersisted = false;
    }
    

    public Task<bool> HasSuccessfulSent(string reference, string type, string templateId)
    {
        return Task.FromResult(_tracker.Any(x => x.REFERENCE == reference && x.TYPE == type && x.TEMPLATE_ID == templateId &&  x.SUCCESS == true));
    }

    public Task RecordSend(string reference, string type, string templateId, bool success, string message)
    {
        _tracker.RemoveAll(x => x.REFERENCE == reference && x.TEMPLATE_ID == type && x.TEMPLATE_ID == templateId);
        
        _tracker.Add(new BlobTrackerModel()
        {
            REFERENCE = reference,
            TYPE = type,
            TEMPLATE_ID = templateId,
            SUCCESS = success,
            MESSAGE = message,
        });
        
        return Task.CompletedTask;
    }

    public async Task Persist(string file)
    {
        var stream = await azureBlobStorage.GetBlobUploadStream(ContainerName, file);
        await WriteAsync(stream);
        _changesPersisted = true;
    }

    private async Task WriteAsync(Stream stream)
    {
        if (_changesPersisted)
        {
            return;
        }

        var dataFactories = new List<DataFactory>
        {
            new DataFactory<BlobTrackerModel, string>(NofityFields.Reference, document => document.REFERENCE),
            new DataFactory<BlobTrackerModel, string>(NofityFields.Type, document => document.TYPE),
            new DataFactory<BlobTrackerModel, string>(NofityFields.TemplateId, document => document.TEMPLATE_ID),
            new DataFactory<BlobTrackerModel, bool>(NofityFields.Success,
                document => document.SUCCESS),
            new DataFactory<BlobTrackerModel, string>(NofityFields.Message, document => document.MESSAGE),
        };

        var schema = new ParquetSchema(dataFactories.Select(df => df.Field).ToArray<Field>());
        await using var writer = await ParquetWriter.CreateAsync(schema, stream);
        using var groupWriter = writer.CreateRowGroup();
        
        foreach (var dataFactory in dataFactories)
        {
            await groupWriter.WriteColumnAsync(dataFactory.CreateColumn(_tracker));
        }
    }
}

public class BlobTrackerModel
{
    public string REFERENCE { get; set; }
    public string TYPE { get; set; }
    public string TEMPLATE_ID { get; set; }
    public bool SUCCESS { get; set; }
    public string MESSAGE { get; set; }
}
