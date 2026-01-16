using Nbs.MeshClient.Responses;
using Refit;

namespace Nbs.MeshClient;

/// <summary>
/// IMeshClient
/// </summary>
[Headers("Authorization: NHSMESH", "Accept: application/vnd.mesh.v2+json")]
public interface IMeshClient
{
    /// <summary>
    /// Validate Async
    /// </summary>
    [Post("/messageexchange/{mailboxId}")]
    Task<IApiResponse> ValidateAsync(string mailboxId);

    /// <summary>
    /// Check Inbox Async
    /// </summary>
    [Get("/messageexchange/{mailboxId}/inbox")]
    Task<IApiResponse<CheckInboxResponse>> CheckInboxAsync(string mailboxId);

    /// <summary>
    ///  Check Inbox Async
    /// </summary>
    [Get("/messageexchange/{mailboxId}/inbox?continue_from={continueFrom}")]
    Task<IApiResponse<CheckInboxResponse>> CheckInboxAsync(string mailboxId, string continueFrom);

    /// <summary>
    /// Get Message Async
    /// </summary>
    [Get("/messageexchange/{mailboxId}/inbox/{messageId}")]
    Task<IApiResponse<string>> GetMessageAsync(string mailboxId, string messageId);

    /// <summary>
    /// Get Message Bytes Async
    /// </summary>
    [Get("/messageexchange/{mailboxId}/inbox/{messageId}")]
    Task<IApiResponse<Stream>> GetMessageBytesAsync(string mailboxId, string messageId);

    /// <summary>
    /// Get Message Part Async
    /// </summary>
    [Get("/messageexchange/{mailboxId}/inbox/{messageId}/{messagePart}")]
    Task<IApiResponse<string>> GetMessagePartAsync(string mailboxId, string messageId, int messagePart);

    /// <summary>
    /// Get Message Part Bytes Async
    /// </summary>
    [Get("/messageexchange/{mailboxId}/inbox/{messageId}/{messagePart}")]
    Task<IApiResponse<Stream>> GetMessagePartBytesAsync(string mailboxId, string messageId, int messagePart);

    /// <summary>
    /// Acknowledge Message Async
    /// </summary>
    [Put("/messageexchange/{mailboxId}/inbox/{messageId}/status/acknowledged")]
    Task<IApiResponse> AcknowledgeMessageAsync(string mailboxId, string messageId);

    /// <summary>
    /// Send Message Async
    /// </summary>
    [Post("/messageexchange/{sourceMailboxId}/outbox")]
    Task<IApiResponse<SendMessageResponse>> SendMessageAsync(string sourceMailboxId,
        [Header("mex-to")] string recipientMailboxId, [Header("mex-workflowid")] string workflowId,
        [Body] HttpContent content, [Header("mex-chunk-range")] string? chunkRange = null,
        [Header("mex-filename")] string? fileName = null);

    /// <summary>
    /// Send Message Part Async
    /// </summary>
    [Post("/messageexchange/{sourceMailboxId}/outbox/{messageId}/{messagePart}")]
    Task<IApiResponse> SendMessagePartAsync(string sourceMailboxId, string messageId, int messagePart,
        [Header("mex-chunk-range")] string chunkRange, [Body] HttpContent content);
}
