
using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum QueryType
    {
        NotSet,

        Days,

        Hours,

        Slots
    }

    
}
