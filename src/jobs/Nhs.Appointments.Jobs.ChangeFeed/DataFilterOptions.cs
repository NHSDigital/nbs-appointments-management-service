namespace Nhs.Appointments.Jobs.ChangeFeed;

public class DataFilterOptions
{
    public List<string>? Sites { get; init; }
    public List<string>? DocumentTypes { get; init; }
}
