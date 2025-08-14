using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nbs.MeshClient;
using Nbs.MeshClient.Auth;

namespace DataExtract;
public class DataExtractWorker<TExtractor>(
    IHostApplicationLifetime hostApplicationLifetime,
    IOptions<MeshSendOptions> meshSendOptions,
    IOptions<MeshAuthorizationOptions> meshAuthOptions,
    IMeshFactory meshFactory,
    TimeProvider timeProvider,
    TExtractor dataExtract,
    IOptions<FileOptions> fileOptions
    ) : BackgroundService where TExtractor : class, IExtractor
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var outputFile = new FileInfo(GenerateFileName());
            await dataExtract.RunAsync(outputFile);

            if (fileOptions.Value.CreateSampleFile)
            {
                WriteFileLocally(outputFile);
            }
            Console.WriteLine($"Data extract completed. Output file: {outputFile.FullName}. Don't send to mesh");
            //await SendViaMesh(outputFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Environment.ExitCode = -1;
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }

    private void WriteFileLocally(FileInfo outputFile) =>
        outputFile.CopyTo($"{fileOptions.Value.FileName}-sample.parquet", true);

    private async Task SendViaMesh(FileInfo fileToSend)
    {
        if (string.IsNullOrEmpty(meshSendOptions.Value.DestinationMailboxId) == false)
        {
            var meshMailbox = meshFactory.GetMailbox(meshAuthOptions.Value.MailboxId);
            var meshFileSender = new MeshFileSender(meshMailbox);
            await meshFileSender.SendFile(fileToSend, meshSendOptions.Value.DestinationMailboxId, meshSendOptions.Value.WorkflowId);
        }
        else
            throw new InvalidOperationException("Destination mailbox was not configured");
    }

    // Adding 00 as a replacement for time zone as the reporting consumers can't consume +/-
    private string GenerateFileName() => $"MYA_{fileOptions.Value.FileName}_{timeProvider.GetUtcNow():yyyyMMddTHHmmss}00.parquet";
}
