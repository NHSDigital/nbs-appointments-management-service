using Nbs.MeshClient;
using Microsoft.Extensions.Options;
using Nbs.MeshClient.Auth;

namespace BookingsDataExtracts;
public class DataExtractWorker(
    IHostApplicationLifetime hostApplicationLifetime,
    IOptions<MeshSendOptions> meshSendOptions,
    IOptions<MeshAuthorizationOptions> meshAuthOptions,
    IMeshFactory meshFactory,
    TimeProvider timeProvider,
    BookingDataExtract bookingDataExtract
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var outputFile = new FileInfo(GenerateFileName());
            await bookingDataExtract.RunAsync(outputFile);

            await SendViaMesh(outputFile);
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

    private string GenerateFileName() => $"MYA_booking_{timeProvider.GetUtcNow():yyyy-MM-ddTHHmm}.parquet";
}
