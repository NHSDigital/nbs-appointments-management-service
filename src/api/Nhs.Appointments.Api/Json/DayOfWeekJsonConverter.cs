using Newtonsoft.Json;
using System;

namespace Nhs.Appointments.Api.Json;

internal class DayOfWeekJsonConverter : JsonConverter<DayOfWeek>
{
    public override DayOfWeek ReadJson(JsonReader reader, Type objectType, DayOfWeek existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), (string)reader.Value);            
    }

    public override void WriteJson(JsonWriter writer, DayOfWeek value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
