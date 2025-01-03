using Nbs.MeshClient.Responses;
using Refit;

namespace Nbs.MeshClient
{
    [Headers("Authorization: NHSMESH", "Accept: application/vnd.mesh.v2+json")]
    public interface IMeshClient
    {
        [Post("/messageexchange/{mailboxId}")]
        public Task<IApiResponse> ValidateAsync(string mailboxId);

        [Get("/messageexchange/{mailboxId}/inbox")]
        public Task<IApiResponse<CheckInboxResponse>> CheckInboxAsync(string mailboxId);

        [Get("/messageexchange/{mailboxId}/inbox?continue_from={continueFrom}")]
        public Task<IApiResponse<CheckInboxResponse>> CheckInboxAsync(string mailboxId, string continueFrom);

        [Get("/messageexchange/{mailboxId}/inbox/{messageId}")]
        public Task<IApiResponse<string>> GetMessageAsync(string mailboxId, string messageId);

        [Get("/messageexchange/{mailboxId}/inbox/{messageId}")]
        public Task<IApiResponse<Stream>> GetMessageBytesAsync(string mailboxId, string messageId);

        [Get("/messageexchange/{mailboxId}/inbox/{messageId}/{messagePart}")]
        public Task<IApiResponse<string>> GetMessagePartAsync(string mailboxId, string messageId, int messagePart);

        [Get("/messageexchange/{mailboxId}/inbox/{messageId}/{messagePart}")]
        public Task<IApiResponse<Stream>> GetMessagePartBytesAsync(string mailboxId, string messageId, int messagePart);

        [Put("/messageexchange/{mailboxId}/inbox/{messageId}/status/acknowledged")]
        public Task<IApiResponse> AcknowledgeMessageAsync(string mailboxId, string messageId);

        [Post("/messageexchange/{sourceMailboxId}/outbox")]
        public Task<IApiResponse<SendMessageResponse>> SendMessageAsync(string sourceMailboxId, [Header("mex-to")] string recipientMailboxId, [Header("mex-workflowid")] string workflowId, [Body] HttpContent content, [Header("mex-chunk-range")] string? chunkRange = null);

        [Post("/messageexchange/{sourceMailboxId}/outbox/{messageId}/{messagePart}")]
        public Task<IApiResponse> SendMessagePartAsync(string sourceMailboxId, string messageId, int messagePart, [Header("mex-chunk-range")] string chunkRange, [Body] HttpContent content);
    }
}
