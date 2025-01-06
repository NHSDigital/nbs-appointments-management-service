using Microsoft.Extensions.Logging;
using Nbs.MeshClient.Errors;
using Refit;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using Nbs.MeshClient.Responses;

namespace Nbs.MeshClient
{
    public partial class MeshMailbox : IMeshMailbox
    {
        private readonly ILogger<MeshMailbox> _logger;
        private readonly IMeshClient _meshClient;

        public MeshMailbox(string mailboxId, ILogger<MeshMailbox> logger, IMeshClient meshClient)
        {
            MailboxId = mailboxId;
            _logger = logger;
            _meshClient = meshClient;
        }

        public string MailboxId { get; }

        public async Task ValidateAsync()
        {
            await _meshClient.ValidateAsync(MailboxId).ConfigureAwait(false);
        }

        private async Task<CheckInboxResponse> CheckInboxAsync(string? continueFrom)
        {
            IApiResponse<CheckInboxResponse> result;
            if (string.IsNullOrEmpty(continueFrom))
            {
                result = await _meshClient.CheckInboxAsync(MailboxId).ConfigureAwait(false);
            }
            else
            {
                result = await _meshClient.CheckInboxAsync(MailboxId, continueFrom).ConfigureAwait(false);
            }
            EnsureSuccessAndContent(result);
            return result.Content!;
        }

        public Task<CheckInboxResponse> CheckInboxAsync()
            => CheckInboxAsync(null);

        public async IAsyncEnumerable<string> GetAllInboxMessageIds()
        {
            CheckInboxResponse result;
            string? continueFrom = null;
            do
            {
                result = await CheckInboxAsync(continueFrom).ConfigureAwait(false);
                foreach (var message in result.Messages)
                {
                    yield return message;
                }

                continueFrom = result.Links?.GetNextContinueFrom();
            } while (continueFrom is not null);
        }

        public async Task ClearMessages()
        {            
            await foreach (var messageId in GetAllInboxMessageIds())
            {
                await AcknowledgeMessageAsync(messageId);
            }
        }

        public async Task<string> GetMessageAsFileAsync(string messageId, DirectoryInfo folder, string fileName = null)
        {
            var result = await _meshClient.GetMessageBytesAsync(MailboxId, messageId);
            fileName = fileName ?? result.Headers.GetValues("mex-filename").FirstOrDefault() ?? messageId;
            var file = new FileInfo(Path.Combine(folder.FullName, fileName));

            EnsureSuccess(result);

            using(var outputStream = file.OpenWrite())
            {
                await result.Content.CopyToAsync(outputStream);

                if (result.StatusCode == HttpStatusCode.PartialContent)
                {
                    if (!result.Headers.TryGetValues("Mex-Chunk-Range", out var rangeValues) || !rangeValues.Any())
                        throw new MeshException("Partial message content contains no chunk range header.");

                    if (rangeValues.Count() > 1)
                        throw new MeshException("Partial message contains multiple chunk range headers.");

                    var rangeHeaderMatch = RangeHeaderValueFormat().Match(rangeValues.Single().Trim());
                    if (!rangeHeaderMatch.Success)
                        throw new MeshException("Partial message chunk range header is in an unrecognised format.");

                    var total = int.Parse(rangeHeaderMatch.Groups["total"].ValueSpan);
                    
                    for (int current = 2; current <= total; current++)
                    {
                        _logger.LogInformation("Retrieving MESH message part {CurrentMessagePart} of {TotalMessageParts}...", current, total);
                        result = await _meshClient.GetMessagePartBytesAsync(MailboxId, messageId, current).ConfigureAwait(false);
                        EnsureSuccessAndContent(result);
                        await result.Content.CopyToAsync(outputStream);
                    }                    
                }
            }

            return fileName;
        }

        public async Task<string> GetMessageAsync(string messageId)
        {
            var result = await _meshClient.GetMessageAsync(MailboxId, messageId).ConfigureAwait(false);
            EnsureSuccessAndContent(result);

            if (result.StatusCode == HttpStatusCode.PartialContent)
            {
                if (!result.Headers.TryGetValues("Mex-Chunk-Range", out var rangeValues) || !rangeValues.Any())
                    throw new MeshException("Partial message content contains no chunk range header.");

                if (rangeValues.Count() > 1)
                    throw new MeshException("Partial message contains multiple chunk range headers.");

                var rangeHeaderMatch = RangeHeaderValueFormat().Match(rangeValues.Single().Trim());
                if (!rangeHeaderMatch.Success)
                    throw new MeshException("Partial message chunk range header is in an unrecognised format.");

                var total = int.Parse(rangeHeaderMatch.Groups["total"].ValueSpan);
                var builder = new StringBuilder(result.Content);

                for (int current = 2; current <= total; current++)
                {
                    _logger.LogInformation("Retrieving MESH message part {CurrentMessagePart} of {TotalMessageParts}...", current, total);
                    result = await _meshClient.GetMessagePartAsync(MailboxId, messageId, current).ConfigureAwait(false);
                    EnsureSuccessAndContent(result);
                    builder.Append(result.Content);
                }

                return builder.ToString();
            }

            return result.Content!;
        }

        public async Task AcknowledgeMessageAsync(string messageId)
        {
            var result = await _meshClient.AcknowledgeMessageAsync(MailboxId, messageId).ConfigureAwait(false);
            EnsureSuccess(result);
        }

        public async Task<string> SendMessageAsync(string recipientMailboxId, string workflowId, HttpContent content, int totalParts = 1)
        {
            string? chunkRange = null;
            if (totalParts < 1)
            {
                throw new ArgumentException("Must be greater than 0.", nameof(totalParts));
            }
            if (totalParts > 1)
            {
                chunkRange = $"1:{totalParts}";
            }

            var result = await _meshClient.SendMessageAsync(MailboxId, recipientMailboxId, workflowId, content, chunkRange).ConfigureAwait(false);
            EnsureSuccessAndContent(result);
            return result.Content!.MessageId ?? throw new InvalidDataException("Message ID was expected but not found.");
        }

        public async Task SendMessagePartAsync(string messageId, int partNumber, int totalParts, HttpContent content)
        {
            if (totalParts < 1)
            {
                throw new ArgumentException("Must be greater than 0.", nameof(totalParts));
            }
            if (partNumber < 1)
            {
                throw new ArgumentException("Must be greater than 0.", nameof(partNumber));
            }
            if (partNumber > totalParts)
            {
                throw new ArgumentException($"Cannot be greater than total parts.", nameof(partNumber));
            }
            var chunkRange = $"{partNumber}:{totalParts}";

            var result = await _meshClient.SendMessagePartAsync(MailboxId, messageId, partNumber, chunkRange, content).ConfigureAwait(false);
            EnsureSuccess(result);
        }

        private void EnsureSuccessAndContent<T>(IApiResponse<T> response)
        {
            var errorPrefix = GetErrorMessagePrefix(response);

            EnsureSuccess(response);

            if (response.Content is null)
            {
                _logger.LogError(
                    "{errorPrefix} response could not be parsed as type '{Type}'.",
                    errorPrefix,
                    typeof(T).FullName);
                throw new MeshException($"{errorPrefix} response could not be parsed as type '{typeof(T).FullName}'.");
            }
        }

        private void EnsureSuccess(IApiResponse response)
        {
            var errorPrefix = GetErrorMessagePrefix(response);

            if (response.Error is not null)
            {
                _logger.LogError(
                    response.Error,
                    "{errorPrefix} resulted in error: {message}",
                    errorPrefix,
                    response.Error.Message);
                throw new MeshException($"{errorPrefix} resulted in error: {response.Error.Message}", response.Error);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "{errorPrefix} status code indicates an error occurred: {StatusCode}",
                    errorPrefix,
                    response.StatusCode);
                throw new MeshException($"{errorPrefix} status code indicates an error occurred: {response.StatusCode}");
            }
        }

        private static string GetErrorMessagePrefix(IApiResponse response)
        {
            var requestMethod = response.RequestMessage!.Method;
            var requestUrl = response.RequestMessage!.RequestUri!.AbsolutePath;
            return $"MESH request '{requestMethod} {requestUrl}'";
        }

        [GeneratedRegex(@"^(?<current>[0-9]+):(?<total>[0-9]+)$")]
        private static partial Regex RangeHeaderValueFormat();
    }
}
