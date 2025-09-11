using Newtonsoft.Json;
using Nhs.Appointments.Persistance.Models;

namespace DataExtract.Documents;

public class NbsBookingDocument : BookingDocument
{
    [JsonProperty("additionalData")]
    public new NbsAdditionalData AdditionalData { get; set; }
}

public record NbsAdditionalData
{
    [JsonProperty("referralType")]
    public string ReferralType { get; set; }
    
    [JsonProperty("source")]
    public string Source { get; set; }

}
