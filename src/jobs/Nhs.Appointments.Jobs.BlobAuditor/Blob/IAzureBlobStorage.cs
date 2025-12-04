namespace Nhs.Appointments.Jobs.BlobAuditor.Blob;

public interface IAzureBlobStorage
{
    Task<Stream> GetBlobUploadStream(string containerName, string blobName);
}
