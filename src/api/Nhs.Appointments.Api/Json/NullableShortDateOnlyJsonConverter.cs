using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Json;

public class NullableShortDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override void WriteJson(JsonWriter writer, DateOnly? value, JsonSerializer serializer)
    {
        writer.WriteValue(value.Value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }

    public override DateOnly? ReadJson(JsonReader reader, Type objectType, DateOnly? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value == null)
        {
            return null;
        }
        return DateOnly.ParseExact((string)reader.Value, DateFormat, CultureInfo.InvariantCulture);
    }
}