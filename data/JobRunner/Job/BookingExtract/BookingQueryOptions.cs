namespace JobRunner.Job.BookingExtract;

public class BookingQueryOptions
{
    public string[] Services { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
}
