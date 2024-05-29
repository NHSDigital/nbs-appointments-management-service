namespace Nhs.Appointments.Core;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}

public class SystemDateTimeProvider: IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}