using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models.Converters
{
    public class DayOfWeekJsonConverter : JsonConverter<DayOfWeek>
    {
        public override DayOfWeek Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DayOfWeek value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }


    public class DayOfWeekListJsonConverter : JsonConverter<DayOfWeek[]>
    {
        public override DayOfWeek[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var options2 = new JsonSerializerOptions(options);
            options2.Converters.Add(new DayOfWeekJsonConverter());

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }
            reader.Read();

            var elements = new List<DayOfWeek>();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                elements.Add(JsonSerializer.Deserialize<DayOfWeek>(ref reader, options2)!);

                reader.Read();
            }

            return elements.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, DayOfWeek[] value, JsonSerializerOptions options)
        {
            var options2 = new JsonSerializerOptions(options);
            options2.Converters.Add(new DayOfWeekJsonConverter());
            writer.WriteStartArray();

            foreach (DayOfWeek item in value)
            {
                JsonSerializer.Serialize(writer, item, options2);
            }

            writer.WriteEndArray();
        }
    }
}
