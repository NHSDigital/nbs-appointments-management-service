using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nhs.Appointments.Api.Availability;

[JsonConverter(typeof(StringEnumConverter))]
public enum QueryType
{
    NotSet,
    [EnumMember(Value = "Days")]
    Days,
    [EnumMember(Value = "Hours")]
    Hours,
    [EnumMember(Value = "Slots")]
    Slots
}