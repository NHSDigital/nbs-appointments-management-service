using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Nbs.MeshClient.Auth
{
    public class MeshAuthorizationTokenGenerator(MeshAuthorizationOptions options) : IMeshAuthorizationTokenGenerator
    {
        private readonly TimeProvider _timeProvider = TimeProvider.System;
        private readonly Func<string> _uniqueIdProvider = () => Guid.NewGuid().ToString();

        public MeshAuthorizationTokenGenerator(IOptions<MeshAuthorizationOptions> options) : this(options.Value) { }

        internal MeshAuthorizationTokenGenerator(MeshAuthorizationOptions options, TimeProvider timeProvider, Func<string> uniqueIdProvider)
            : this(options)
        {
            _timeProvider = timeProvider;
            _uniqueIdProvider = uniqueIdProvider;
        }

        protected string MailboxId { get; } = options.MailboxId;
        protected string MailboxPassword { get; } = options.MailboxPassword;
        protected string SharedKey { get; } = options.SharedKey;

        public string GenerateAuthorizationToken()
        {
            var nonce = _uniqueIdProvider();
            var timestamp = _timeProvider.GetUtcNow().ToString("yyyyMMddHHmm");
            var message = GetMessage(timestamp, nonce);
            var hash = GetHash(message);

            var result = $"{MailboxId}:{nonce}:0:{timestamp}:{hash}";
            return result;
        }

        protected internal string GetMessage(string timestamp, string nonce)
        {
            return $"{MailboxId}:{nonce}:0:{MailboxPassword}:{timestamp}";
        }

        protected internal string GetHash(string message)
        {
            Console.WriteLine(MailboxPassword.Length);
            Console.WriteLine(SharedKey.Length);

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SharedKey)))
            {
                return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(message))).Replace("-", "").ToLowerInvariant();
            }              
        }
    }
}
