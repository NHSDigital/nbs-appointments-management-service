using Nbs.MeshClient.Responses;

namespace Nbs.MeshClient
{
    public interface IMeshMailbox
    {
        /// <summary>
        /// The ID of the mailbox.
        /// </summary>
        string MailboxId { get; }

        Task ValidateAsync();

        /// <summary>
        /// Retrieves a collection of message IDs in the inbox.
        /// </summary>
        /// <remarks>
        /// This method will only return up to 500 message IDs.
        /// </remarks>
        /// <returns>Details of the check inbox response.</returns>
        Task<CheckInboxResponse> CheckInboxAsync();

        /// <summary>
        /// Retrieves a collection of message IDs in the inbox.
        /// </summary>
        /// <returns>The message IDs in the inbox.</returns>
        IAsyncEnumerable<string> GetAllInboxMessageIds();

        /// <summary>
        /// Retrieves the contents of a message.
        /// </summary>
        /// <param name="messageId">The ID of the message.</param>
        /// <returns>The contents of the message.</returns>
        Task<string> GetMessageAsync(string messageId);

        /// <summary>
        /// Retrieves the contents of a message and writes them to a file.
        /// </summary>
        /// <param name="messageId">The ID of the message.</param>        
        /// <param name="file">The file to be written to</param>
        Task GetMessageAsFileAsync(string messageId, FileInfo file);

        /// <summary>
        /// Acknowledges receipt of a message.
        /// </summary>
        /// <param name="messageId">The ID of the message.</param>
        Task AcknowledgeMessageAsync(string messageId);

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="recipientMailboxId">The ID of the mailbox receiving the message.</param>
        /// <param name="workflowId">The ID of the workflow for the message.</param>
        /// <param name="content">The content of the message.</param>
        /// <param name="totalParts">The total number of parts to the message.</param>
        /// <returns>The ID of the message.</returns>
        Task<string> SendMessageAsync(string recipientMailboxId, string workflowId, HttpContent content, int totalParts = 1);

        /// <summary>
        /// Sends a message part.
        /// </summary>
        /// <param name="messageId">The ID of the message to continue sending.</param>
        /// <param name="partNumber">The part number of the message (starting from 1).</param>
        /// <param name="totalParts">The total number of parts to the message.</param>
        /// <param name="content">The content of the message.</param>
        Task SendMessagePartAsync(string messageId, int partNumber, int totalParts, HttpContent content);
    }
}
