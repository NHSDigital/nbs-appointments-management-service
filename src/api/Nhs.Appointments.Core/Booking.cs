﻿using Newtonsoft.Json;

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

    [JsonProperty("sessionHolder")]
    public string SessionHolder { get; set; }
    
    [JsonProperty("outcome")]
    public string Outcome { get; set; }
    
    [JsonProperty("attendeeDetails")]
    public AttendeeDetails AttendeeDetails { get; set; }

    [JsonProperty("contactDetails")]
    public ContactDetails ContactDetails { get; set; }
    
    [JsonIgnore]
    public TimePeriod TimePeriod => new TimePeriod(From, TimeSpan.FromMinutes(Duration));
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

public class ContactDetails
{ 
    [JsonProperty("email")]
    public string Email { get; set; }
    [JsonProperty("phoneNumber")]
    public string PhoneNumber { get; set; }
    [JsonProperty("emailContactConsent")]
    public bool EmailContactConsent { get; set; }
    [JsonProperty("phoneContactConsent")]
    public bool PhoneContactConsent { get; set; }
}