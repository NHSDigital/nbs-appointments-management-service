// See https://aka.ms/new-console-template for more information

using Azure.Storage.Blobs;

var containerName = args.GetValue(0);

var blobClient = new BlobServiceClient(
    "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;");

var container = blobClient.GetBlobContainerClient(containerName.ToString());

var blobs = container.GetBlobs();

foreach (var blob in blobs)
{
    var client = container.GetBlobClient(blob.Name);
    client.DownloadTo(blob.Name);
}


