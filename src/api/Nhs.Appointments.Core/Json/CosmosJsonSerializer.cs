using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Nhs.Appointments.Core.Json;

public class CosmosJsonSerializer : CosmosSerializer
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Converters =
        {
            new ShortTimeOnlyJsonConverter(), 
            new ShortDateOnlyJsonConverter(), 
            new DayOfWeekJsonConverter(), 
            new Newtonsoft.Json.Converters.StringEnumConverter()
        }
    };
    
    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            var body = new StreamReader(stream).ReadToEndAsync().GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<T>(body, JsonSerializerSettings);
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var json = JsonConvert.SerializeObject(input, JsonSerializerSettings);
        return new MemoryStream(Encoding.UTF8.GetBytes(json));
    }
}
