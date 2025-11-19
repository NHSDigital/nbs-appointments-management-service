namespace Nhs.Appointments.Core.Availability;
public class AvailabilityByDays
{
    public string Site {  get; set; }
    public List<DayEntry> Days { get; set; }
}

public class DayEntry
{
    public DateOnly Date { get; set; }
    public List<Block> Blocks { get; set; }
}

public class Block
{
    public string From { get; set; }
    public string Until {  get; set; }
}
