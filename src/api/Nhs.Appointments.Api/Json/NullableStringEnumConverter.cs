using Newtonsoft.Json;
using System;

namespace Nhs.Appointments.Api.Json;
public class NullableStringEnumConverter<T> : JsonConverter<T?> where T : struct, Enum
{
    public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        if (reader.TokenType == JsonToken.String)
        {
            var enumText = reader.Value?.ToString();
            if (Enum.TryParse(enumText, true, out T parsedEnum))
                return parsedEnum;

            JsonDeserializationContext.AddError(reader.Path, $"Invalid value '{enumText}' for enum '{typeof(T).Name}'");
            return null;
        }

        JsonDeserializationContext.AddError(reader.Path, $"Unexpected token {reader.TokenType} for enum '{typeof(T).Name}'");
        return null;
    }

    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
    {
        if (value.HasValue)
            writer.WriteValue(value.Value.ToString());
        else
            writer.WriteNull();
    }
}
