using Newtonsoft.Json;
using Nhs.Appointments.Core;
using System.Globalization;
using System.Text;

namespace CsvDataTool;

public class SiteJsonWriter(FileInfo output)
{
    public async Task Write(Site[] sites)
    {
        var serializerSettings = new JsonSerializerSettings
        {
            Converters = { new ShortTimeOnlyJsonConverter(), new ShortDateOnlyJsonConverter(), new DayOfWeekJsonConverter(), new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        var json = JsonConvert.SerializeObject(sites, serializerSettings);

        using(var writer = output.OpenWrite())
        {
            await writer.WriteAsync(Encoding.UTF8.GetBytes(json));
        }
    }
}

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

public class ShortDateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return DateOnly.ParseExact((string)reader.Value, DateFormat, CultureInfo.InvariantCulture);
    }

    public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }
}

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
