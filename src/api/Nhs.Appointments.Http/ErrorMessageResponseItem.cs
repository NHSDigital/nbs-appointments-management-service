namespace Nhs.Appointments.Http;

public class ErrorMessageResponseItem
{
    public string Message { get; set; }
    public string Property { get; set; }

    public static IReadOnlyCollection<ErrorMessageResponseItem> None => Enumerable.Empty<ErrorMessageResponseItem>().ToList().AsReadOnly();
}
