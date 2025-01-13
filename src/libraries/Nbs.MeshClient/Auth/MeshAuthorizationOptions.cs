namespace Nbs.MeshClient.Auth
{
    public class MeshAuthorizationOptions
    {
        public required string MailboxId { get; set; }
        public required string MailboxPassword { get; set; }
        public required string SharedKey { get; set; }
        public required string CertificateName { get; set; }
        
    }
}
