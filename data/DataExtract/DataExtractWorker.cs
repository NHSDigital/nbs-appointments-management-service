using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DataExtract;
public class DataExtractWorker<TExtractor>(
    IHostApplicationLifetime hostApplicationLifetime,
    TimeProvider timeProvider,
    IOptions<FileOptions> fileOptions,
    IServiceProvider serviceProvider,
    IFileSender fileSender
    ) : BackgroundService where TExtractor : class, IExtractor
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();

            var dataExtract = scope.ServiceProvider.GetRequiredService<TExtractor>();

            var outputFile = new FileInfo(GenerateFileName());
            await dataExtract.RunAsync(outputFile);

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

    private async Task SendFile(
        FileInfo fileToSend)
    {
        await fileSender.SendFile(fileToSend);
    }

    // Adding 00 as a replacement for time zone as the reporting consumers can't consume +/-
    private string GenerateFileName() => $"MYA_{fileOptions.Value.FileName}_{timeProvider.GetUtcNow():yyyyMMddTHHmmss}00.parquet";
}
