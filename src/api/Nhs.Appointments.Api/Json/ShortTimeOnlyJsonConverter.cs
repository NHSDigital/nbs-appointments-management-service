using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Nhs.Appointments.Api.Json;

public class ShortTimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string TimeFormat = "HH:mm";

    public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return TimeOnly.ParseExact((string)reader.Value, TimeFormat, CultureInfo.InvariantCulture);
    }

    public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(TimeFormat, CultureInfo.InvariantCulture));
    }
}
