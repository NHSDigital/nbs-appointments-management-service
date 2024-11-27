using Azure.Storage.Blobs;
using Nhs.Appointments.Core;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Persistance.Options;

namespace Nhs.Appointments.Persistance;

public class EulaStore : IEulaStore
{
    private readonly EulaStoreOptions _options;
    private readonly BlobServiceClient _blobServiceClient;

    public EulaStore(IOptions<EulaStoreOptions> options, BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        _options = options.Value;
    }



    public async Task<EulaVersion> GetLatestVersion()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient("eula");

        var eulaBlob = (await blobClient.DownloadContentAsync()).Value;

        var versionDate = DateOnly.FromDateTime(DateTime.Parse(eulaBlob.Details.VersionId));
        // TODO: Do we need to make this a stream or is enumerating it all here sufficient?
        var content = eulaBlob.Content.ToString();

        return new EulaVersion()
        {
            VersionDate = versionDate,
            Content = content
        };
    }
}