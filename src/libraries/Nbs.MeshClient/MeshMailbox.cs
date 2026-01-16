using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Nbs.MeshClient.Errors;
using Nbs.MeshClient.Responses;
using Refit;

namespace Nbs.MeshClient;

/// <summary>
///     MeshMailbox
/// </summary>
public partial class MeshMailbox(string mailboxId, ILogger<MeshMailbox> logger, IMeshClient meshClient)
    : IMeshMailbox
{
    /// <inheritdoc />
    public string MailboxId { get; } = mailboxId;

    /// <inheritdoc />
    public async Task ValidateAsync()
    {
        await meshClient.ValidateAsync(MailboxId).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<CheckInboxResponse> CheckInboxAsync()
        => CheckInboxAsync(null);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<string> GetMessageAsFileAsync(string messageId, DirectoryInfo folder, string? fileName = null)
    {
        var result = await meshClient.GetMessageBytesAsync(MailboxId, messageId);
        fileName ??= result.Headers.GetValues("mex-filename").FirstOrDefault() ?? messageId;
        var file = new FileInfo(Path.Combine(folder.FullName, fileName));

        EnsureSuccess(result);

        using (var outputStream = file.OpenWrite())
        {
            await result.Content?.CopyToAsync(outputStream)!;

            if (result.StatusCode == HttpStatusCode.PartialContent)
            {
                if (!result.Headers.TryGetValues("Mex-Chunk-Range", out var rangeValues) || !rangeValues.Any())
                {
                    throw new MeshException("Partial message content contains no chunk range header.");
                }

                if (rangeValues.Count() > 1)
                {
                    throw new MeshException("Partial message contains multiple chunk range headers.");
                }

                var rangeHeaderMatch = RangeHeaderValueFormat().Match(rangeValues.Single().Trim());
                if (!rangeHeaderMatch.Success)
                {
                    throw new MeshException("Partial message chunk range header is in an unrecognised format.");
                }

                var total = int.Parse(rangeHeaderMatch.Groups["total"].ValueSpan);

                for (var current = 2; current <= total; current++)
                {
                    logger.LogInformation("Retrieving MESH message part {CurrentMessagePart} of {TotalMessageParts}...",
                        current, total);
                    result = await meshClient.GetMessagePartBytesAsync(MailboxId, messageId, current)
                        .ConfigureAwait(false);
                    EnsureSuccessAndContent(result);
                    await result.Content?.CopyToAsync(outputStream)!;
                }
            }
        }

        return fileName;
    }

    /// <inheritdoc />
    public async Task<string> GetMessageAsync(string messageId)
    {
        var result = await meshClient.GetMessageAsync(MailboxId, messageId).ConfigureAwait(false);
        EnsureSuccessAndContent(result);

        if (result.StatusCode == HttpStatusCode.PartialContent)
        {
            if (!result.Headers.TryGetValues("Mex-Chunk-Range", out var rangeValues) || !rangeValues.Any())
            {
                throw new MeshException("Partial message content contains no chunk range header.");
            }

            if (rangeValues.Count() > 1)
            {
                throw new MeshException("Partial message contains multiple chunk range headers.");
            }

            var rangeHeaderMatch = RangeHeaderValueFormat().Match(rangeValues.Single().Trim());
            if (!rangeHeaderMatch.Success)
            {
                throw new MeshException("Partial message chunk range header is in an unrecognised format.");
            }

            var total = int.Parse(rangeHeaderMatch.Groups["total"].ValueSpan);
            var builder = new StringBuilder(result.Content);

            for (var current = 2; current <= total; current++)
            {
                logger.LogInformation("Retrieving MESH message part {CurrentMessagePart} of {TotalMessageParts}...",
                    current, total);
                result = await meshClient.GetMessagePartAsync(MailboxId, messageId, current).ConfigureAwait(false);
                EnsureSuccessAndContent(result);
                builder.Append(result.Content);
            }

            return builder.ToString();
        }

        return result.Content!;
    }

    /// <inheritdoc />
    public async Task AcknowledgeMessageAsync(string messageId)
    {
        var result = await meshClient.AcknowledgeMessageAsync(MailboxId, messageId).ConfigureAwait(false);
        EnsureSuccess(result);
    }

    /// <inheritdoc />
    public async Task<string> SendMessageAsync(string recipientMailboxId, string workflowId, HttpContent content,
        int totalParts = 1, string? fileName = null)
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

        var result = await meshClient
            .SendMessageAsync(MailboxId, recipientMailboxId, workflowId, content, chunkRange, fileName)
            .ConfigureAwait(false);
        EnsureSuccessAndContent(result);
        return result.Content!.MessageId ?? throw new InvalidDataException("Message ID was expected but not found.");
    }

    /// <inheritdoc />
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
            throw new ArgumentException("Cannot be greater than total parts.", nameof(partNumber));
        }

        var chunkRange = $"{partNumber}:{totalParts}";

        var result = await meshClient.SendMessagePartAsync(MailboxId, messageId, partNumber, chunkRange, content)
            .ConfigureAwait(false);
        EnsureSuccess(result);
    }

    private async Task<CheckInboxResponse> CheckInboxAsync(string? continueFrom)
    {
        IApiResponse<CheckInboxResponse> result;
        if (string.IsNullOrEmpty(continueFrom))
        {
            result = await meshClient.CheckInboxAsync(MailboxId).ConfigureAwait(false);
        }
        else
        {
            result = await meshClient.CheckInboxAsync(MailboxId, continueFrom).ConfigureAwait(false);
        }

        EnsureSuccessAndContent(result);
        return result.Content!;
    }

    /// <summary>
    /// Clear Messages
    /// </summary>
    public async Task ClearMessages()
    {
        await foreach (var messageId in GetAllInboxMessageIds())
        {
            await AcknowledgeMessageAsync(messageId);
        }
    }

    private void EnsureSuccessAndContent<T>(IApiResponse<T> response)
    {
        var errorPrefix = GetErrorMessagePrefix(response);

        EnsureSuccess(response);

        if (response.Content is null)
        {
            logger.LogError(
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
            logger.LogError(
                response.Error,
                "{errorPrefix} resulted in error: {message}",
                errorPrefix,
                response.Error.Message);
            throw new MeshException($"{errorPrefix} resulted in error: {response.Error.Message}", response.Error);
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
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
