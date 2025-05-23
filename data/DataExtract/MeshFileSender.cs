using Nbs.MeshClient;

namespace DataExtract;
public class MeshFileSender(IMeshMailbox meshMailbox)
{
    private const int chunkSizeBytes = 100_000_000;
    public async Task SendFile(FileInfo file, string destinationMailBox, string workflowId)
    {
        var messageId = string.Empty;
        var totalChunks = (int)Math.Ceiling(file.Length / (float)chunkSizeBytes);
        using (var fileStream = file.OpenRead())
        {
            for (var chunksSent = 0; chunksSent < totalChunks; chunksSent++)
            {
                var bytesToRead = Math.Min(chunkSizeBytes, file.Length - fileStream.Position);
                var data = new byte[bytesToRead];
                var bytesRead = fileStream.Read(data, (int)fileStream.Position, (int)bytesToRead);
                var content = new ByteArrayContent(data, 0, bytesRead);
                if (chunksSent == 0)
                    messageId = await meshMailbox.SendMessageAsync(destinationMailBox, workflowId, content, totalChunks, file.Name);
                else
                    await meshMailbox.SendMessagePartAsync(messageId, chunksSent + 1, totalChunks, content);
            }
        }
    }
}
