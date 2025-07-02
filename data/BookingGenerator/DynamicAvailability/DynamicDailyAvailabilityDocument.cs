using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator.DynamicAvailability;

[CosmosDocumentType("dynamic_daily_availability")]
public class DynamicDailyAvailabilityDocument : TypedCosmosDocument
{
    public int DaysToAdd { get; set; }
    public bool GenerateBookings { get; set; }
    public bool RandomServices { get; set; }
    public DailyAvailabilityDocument AvailabilityTemplate { get; set; }
    public BookingDocument BookingTemplate { get; set; }
}
