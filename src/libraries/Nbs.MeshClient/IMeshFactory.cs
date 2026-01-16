namespace Nbs.MeshClient;

/// <summary>
/// MeshFactory interface
/// </summary>
public interface IMeshFactory
{
    /// <summary>
    /// Get Mailbox
    /// </summary>
    IMeshMailbox GetMailbox(string mailboxId);
}
