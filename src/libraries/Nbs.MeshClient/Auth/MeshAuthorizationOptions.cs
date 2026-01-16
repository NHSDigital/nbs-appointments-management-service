namespace Nbs.MeshClient.Auth;

/// <summary>
/// MeshAuthorizationOptions
/// </summary>
public class MeshAuthorizationOptions
{
    /// <summary>
    /// Mailbox Id
    /// </summary>
    public required string MailboxId { get; set; }
    /// <summary>
    /// Mailbox Password
    /// </summary>
    public required string MailboxPassword { get; set; }
    /// <summary>
    /// Shared Key
    /// </summary>
    public required string SharedKey { get; set; }
    /// <summary>
    /// Certificate Name
    /// </summary>
    public required string CertificateName { get; set; }
}
