namespace Nhs.Appointments.Core.Messaging.Events;

public class BookingMade
{
    public string Reference { get; set; }
    public DateTime From { get; set; }
    public string Service { get; set; }
    public string Site { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool EmailContactConsent { get; set; }
    public bool PhoneContactConsent { get; set; }

}
