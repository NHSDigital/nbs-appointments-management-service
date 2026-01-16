using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Nbs.MeshClient.Auth;

/// <summary>
///     MeshAuthorizationTokenGenerator
/// </summary>
public class MeshAuthorizationTokenGenerator(MeshAuthorizationOptions options) : IMeshAuthorizationTokenGenerator
{
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    private readonly Func<string> _uniqueIdProvider = () => Guid.NewGuid().ToString();

    /// <inheritdoc />
    public MeshAuthorizationTokenGenerator(IOptions<MeshAuthorizationOptions> options) : this(options.Value) { }

    internal MeshAuthorizationTokenGenerator(MeshAuthorizationOptions options, TimeProvider timeProvider,
        Func<string> uniqueIdProvider)
        : this(options)
    {
        _timeProvider = timeProvider;
        _uniqueIdProvider = uniqueIdProvider;
    }

    private string MailboxId { get; } = options.MailboxId;
    private string MailboxPassword { get; } = options.MailboxPassword;
    private string SharedKey { get; } = options.SharedKey;

    /// <inheritdoc />
    public string GenerateAuthorizationToken()
    {
        var nonce = _uniqueIdProvider();
        var timestamp = _timeProvider.GetUtcNow().ToString("yyyyMMddHHmm");
        var message = GetMessage(timestamp, nonce);
        var hash = GetHash(message);

        var result = $"{MailboxId}:{nonce}:0:{timestamp}:{hash}";
        return result;
    }

    private string GetMessage(string timestamp, string nonce)
    {
        return $"{MailboxId}:{nonce}:0:{MailboxPassword}:{timestamp}";
    }

    private string GetHash(string message)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SharedKey));
        return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(message))).Replace("-", "")
            .ToLowerInvariant();
    }
}
