using Newtonsoft.Json;

namespace BookingsDataExtracts.Documents;

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
