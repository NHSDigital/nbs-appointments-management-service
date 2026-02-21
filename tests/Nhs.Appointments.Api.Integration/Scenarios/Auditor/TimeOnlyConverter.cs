using Newtonsoft.Json;
using System;

namespace Nhs.Appointments.Api.Integration.Scenarios.Auditor;

public class TimeOnlyConverter : JsonConverter<TimeOnly>
{
    private const string Format = "HH:mm";

    public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer)
        => writer.WriteValue(value.ToString(Format));

    public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
        => TimeOnly.Parse((string)reader.Value);
}
