namespace Nhs.Appointments.Persistance.BackoffStrategies;

[Serializable]
internal class BackoffException : Exception
{
    public BackoffException() { }
    public BackoffException(string message) : base(message) { }
    public BackoffException(string message, Exception inner) : base(message, inner) { }
}
