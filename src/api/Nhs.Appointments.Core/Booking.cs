using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public class Booking
{
    [JsonProperty("reference")]
    public string Reference { get; set; }

    [JsonProperty("from")]
    public DateTime From { get; set; }
    
    [JsonProperty("duration")]
    public int Duration { get; set; }
    
    [JsonProperty("service")]
    public string Service { get; set; }

    [JsonProperty("site")]
    public string Site { get; set; }
    
    [JsonProperty("status")]
    public AppointmentStatus Status{ get; set; }

    [JsonProperty("availabilityStatus")] public AvailabilityStatus? AvailabilityStatus { get; set; }
    
    [JsonProperty("attendeeDetails")]
    public AttendeeDetails AttendeeDetails { get; set; }

    [JsonProperty("contactDetails")]
    public ContactItem[] ContactDetails { get; set; }

    [JsonProperty("reminderSent")]
    public bool ReminderSent { get; set; }

    [JsonProperty("created")]
    public DateTimeOffset Created { get; set; }    
    
    [JsonIgnore]
    public TimePeriod TimePeriod => new TimePeriod(From, TimeSpan.FromMinutes(Duration));

    [JsonProperty("additionalData")]
    public object AdditionalData { get; set; }


    [JsonProperty("cancellationReason")]
    public CancellationReason? CancellationReason { get; set; }

    [JsonProperty("cancellationNotificationStatus")]
    public CancellationNotificationStatus? CancellationNotificationStatus { get; set; }

    /// <summary>
    ///     When not null, indicates that this booking was made as a Joint Booking and records the number of bookings that were
    ///     made jointly.
    /// </summary>
    [JsonProperty("bookingBatchSize")]
    public int? BookingBatchSize { get; set; }
}

public class AttendeeDetails
{
    [JsonProperty("nhsNumber")]
    public string NhsNumber { get; set; }
    [JsonProperty("firstName")]
    public string FirstName { get; set; }
    [JsonProperty("lastName")]
    public string LastName { get; set; }
    [JsonProperty("dateOfBirth")]
    public DateOnly DateOfBirth { get; set; }
}

public class ContactItem
{
    [JsonProperty("type", Required = Required.Always)]
    public ContactItemType Type { get; set; }

    [JsonProperty("value", Required = Required.Always)]
    public string Value { get; set; }
}

public enum ContactItemType
{ 
    Phone,
    Email,
    Landline
}


public enum AppointmentStatus
{
    Unknown,
    Provisional,
    Booked,
    Cancelled
}

public enum AvailabilityStatus
{
    Unknown,
    Supported,
    Orphaned
}

/// <summary>
/// If this booking has been cancelled, records whether the citizen has been notified of the cancellation.
/// </summary>
public enum CancellationNotificationStatus
{
    Unknown,
    Unnotified,

    // TODO: When requirement comes in to capture automatic notifications which fail,
    // this is how that will be recorded
    AutomaticNotificationFailed,

    // TODO: When the requirement comes in to "tick off" these appointments as the citizens have been manually notified,
    // this is how that will be recorded
    Notified
}

public enum CancellationReason
{
    CancelledByCitizen,
    CancelledBySite,
    RescheduledByCitizen
}
