using Azure.Storage.Blobs;

namespace Nhs.Appointments.Core.Blob;

public interface IAzureBlobStorage
{
    Task<Stream> GetBlobUploadStream(string containerName, string blobName);

    BlobClient GetBlobClientFromContainerAndBlobName(string containerName, string blobName);
}
