using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nbs.MeshClient;
using Nbs.MeshClient.Auth;

namespace DataExtract;
public class DataExtractWorker<TExtractor>(
    IHostApplicationLifetime hostApplicationLifetime,
    IOptions<MeshSendOptions> meshSendOptions,
    IOptions<MeshAuthorizationOptions> meshAuthOptions,
    TimeProvider timeProvider,
    IOptions<FileOptions> fileOptions,
    IOptions<FileSenderOptions> fileSenderOptions,
    IOptions<MeshFileOptions> meshFileOptions,
    IServiceProvider serviceProvider
    ) : BackgroundService where TExtractor : class, IExtractor
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();

            var dataExtract = scope.ServiceProvider.GetRequiredService<TExtractor>();
            var fileSenderFactory = scope.ServiceProvider.GetRequiredService<IFileSenderFactory>();

            var outputFile = new FileInfo(GenerateFileName());
            await dataExtract.RunAsync(outputFile);

            if (fileOptions.Value.CreateSampleFile)
            {
                WriteFileLocally(outputFile);
            }

            await SendFile(outputFile, fileSenderFactory);
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

    private async Task SendFile(
        FileInfo fileToSend,
        IFileSenderFactory fileSenderFactory)
    {
        var senderType = fileSenderOptions.Value.Type;
        if (string.Equals(senderType, "mesh", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(meshFileOptions.Value.DestinationMailboxId))
        {
            throw new InvalidOperationException("Destination mailbox was not configured");
        }

        var sender = fileSenderFactory.Create(senderType);

        await sender.SendFile(fileToSend);
    }

    // Adding 00 as a replacement for time zone as the reporting consumers can't consume +/-
    private string GenerateFileName() => $"MYA_{fileOptions.Value.FileName}_{timeProvider.GetUtcNow():yyyyMMddTHHmmss}00.parquet";
}
