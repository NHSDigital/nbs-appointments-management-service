namespace Nhs.Appointments.Core.Messaging.Events;

public class BookingMade
{
    public string Reference { get; set; }
    public DateTime From { get; set; }
    public string Service { get; set; }
    public string Site { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ContactItem[] ContactDetails { get; set; }

}