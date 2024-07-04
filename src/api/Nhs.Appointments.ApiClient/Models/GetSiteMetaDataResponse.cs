using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record GetSiteMetaDataResponse([property: JsonPropertyName("site")] string Site, [property: JsonPropertyName("additionalInformation")] string AdditionalInformation);
}
