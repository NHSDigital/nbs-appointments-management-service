using Microsoft.Extensions.Logging;

namespace Nbs.MeshClient
{
    public class MeshFactory(ILoggerFactory loggerFactory, IMeshClient meshClient) : IMeshFactory
    {
        public IMeshMailbox GetMailbox(string mailboxId)
        {
            return new MeshMailbox(mailboxId, loggerFactory.CreateLogger<MeshMailbox>(), meshClient);
        }
    }
}
