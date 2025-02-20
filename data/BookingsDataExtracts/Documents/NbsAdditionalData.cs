using Newtonsoft.Json;
using Nhs.Appointments.Persistance.Models;

namespace BookingsDataExtracts.Documents;

public class NbsBookingDocument : BookingDocument
{
    [JsonProperty("additionalData")]
    public new NbsAdditionalData AdditionalData { get; set; }
}

public record NbsAdditionalData
{
    [JsonProperty("referralType")]
    public string ReferralType { get; set; }

    [JsonProperty("isAppBooking")]
    public bool IsAppBooking { get; set; }

    [JsonProperty("isCallCentreBooking")]
    public bool IsCallCentreBooking { get; set; }

    [JsonIgnore]
    public string Source => IsAppBooking ? "NHS App" : IsCallCentreBooking ? "NHS Call Centre" : "NBS";

}
