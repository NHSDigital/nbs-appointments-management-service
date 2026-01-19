using Microsoft.Extensions.Logging;

namespace Nbs.MeshClient;

/// <summary>
/// Mesh Factory
/// </summary>
public class MeshFactory(ILoggerFactory loggerFactory, IMeshClient meshClient) : IMeshFactory
{
    /// <inheritdoc />
    public IMeshMailbox GetMailbox(string mailboxId)
    {
        return new MeshMailbox(mailboxId, loggerFactory.CreateLogger<MeshMailbox>(), meshClient);
    }
}
