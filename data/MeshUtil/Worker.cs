using Microsoft.Extensions.Options;
using Nbs.MeshClient.Auth;
using Nbs.MeshClient;

namespace MeshUtil;
public class MeshUtilWorker(
    IHostApplicationLifetime hostApplicationLifetime,
    IOptions<MeshAuthorizationOptions> meshAuthOptions,
    IMeshFactory meshFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var meshMailbox = meshFactory.GetMailbox(meshAuthOptions.Value.MailboxId);

            Console.WriteLine("Reading mesh mailbox");
            var meshResponse = await meshMailbox.CheckInboxAsync();

            Console.WriteLine($"{meshResponse.Messages.Count} messages found");

            var outputFolder = new DirectoryInfo("./recieved");
            outputFolder.Create();

            foreach (var messgeId in meshResponse.Messages)
            {
                Console.WriteLine($"Reading message {messgeId}");
                var fileName = await meshMailbox.GetMessageAsFileAsync(messgeId, outputFolder);
                await meshMailbox.AcknowledgeMessageAsync(messgeId);
                Console.WriteLine($"File downloaded file {fileName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }        
    }    
}
