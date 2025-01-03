using Nbs.MeshClient;
using Microsoft.Extensions.Options;
using Nbs.MeshClient.Auth;

namespace BookingsDataExtracts;
public class DataExtractWorker(
    IHostApplicationLifetime hostApplicationLifetime,
    IOptions<MeshSendOptions> meshSendOptions,
    IOptions<MeshAuthorizationOptions> meshAuthOptions,
    IMeshFactory meshFactory,
    BookingDataExtract bookingDataExtract
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var outputFile = new FileInfo($"MYA_booking_{DateTime.UtcNow:yyyy-MM-ddTHHmm}.parquet");        
        await bookingDataExtract.RunAsync(outputFile);

        await SendViaMesh(outputFile);

        hostApplicationLifetime.StopApplication();
    }

    private async Task SendViaMesh(FileInfo fileToSend)
    {
        if (string.IsNullOrEmpty(meshSendOptions.Value.DestinationMailboxId) == false)
        {
            //services.Configure<AzureKeyVaultConfiguration>(configuration.GetSection(AzureKeyVaultConfiguration.DefaultConfigSectionName));
            //services.AddSingleton<ICertificateProvider, KeyVaultCertificateProvider>();
            
            var meshMailbox = meshFactory.GetMailbox(meshAuthOptions.Value.MailboxId);
            var meshFileSender = new MeshFileSender(meshMailbox);
            await meshFileSender.SendFile(fileToSend, meshSendOptions.Value.DestinationMailboxId, meshSendOptions.Value.WorkflowId);
        }
    }
}
