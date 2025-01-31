namespace Nbs.MeshClient
{
    public interface IMeshFactory
    {
        IMeshMailbox GetMailbox(string mailboxId);
    }
}
