using CsvHelper;
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
    IOptions<FileOptions> fileOptions,
    IOptions<FileSenderOptions> fileSenderOptions,
    IFileSenderFactory fileSenderFactory, 
    IServiceProvider serviceProvider
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

            await SendFile(outputFile);
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

    private async Task SendFile(FileInfo fileToSend)
    {
        if (string.IsNullOrEmpty(meshSendOptions.Value.DestinationMailboxId) == false)
        {
            var senderType = fileSenderOptions.Value.Type;
            var factory = new FileSenderFactory(serviceProvider);
            var sender = factory.Create(senderType);

            await sender.SendFile(fileToSend);
        }
        else
            throw new InvalidOperationException("Destination mailbox was not configured");
    }

    // Adding 00 as a replacement for time zone as the reporting consumers can't consume +/-
    private string GenerateFileName() => $"MYA_{fileOptions.Value.FileName}_{timeProvider.GetUtcNow():yyyyMMddTHHmmss}00.parquet";
}
