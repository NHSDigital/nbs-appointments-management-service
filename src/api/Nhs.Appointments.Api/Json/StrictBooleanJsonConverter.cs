using System;
using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Json;

public class StrictBooleanJsonConverter : JsonConverter<Boolean>
{
    public override void WriteJson(JsonWriter writer, Boolean value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }

    public override Boolean ReadJson(JsonReader reader, Type objectType, Boolean existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        return Boolean.Parse(reader.Value.ToString());            
    }
}    
