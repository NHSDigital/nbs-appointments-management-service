namespace Nbs.MeshClient.Errors
{
    public class MeshException : Exception
    {
        public MeshException(string message) : base(message)
        {
        }

        public MeshException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
