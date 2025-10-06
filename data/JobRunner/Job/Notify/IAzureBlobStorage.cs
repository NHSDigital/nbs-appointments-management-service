namespace JobRunner.Job.Notify;

public interface IAzureBlobStorage
{
    Task<Stream?> GetBlob(string containerName, string blobName);
    Task<Stream> GetBlobUploadStream(string containerName, string blobName);
}
