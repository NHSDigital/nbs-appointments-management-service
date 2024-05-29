using System;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Json;

public static class JsonRequestReader
{
    public static async Task<(bool, TRequest)> TryReadRequestAsync<TRequest>(Stream stream)
    {
        try
        {
            var request = await ReadRequestAsync<TRequest>(stream);
            return (true, request);
        }
        catch (FormatException)
        {
            return (false, default);
        }
    }

    public static async Task<TRequest> ReadRequestAsync<TRequest>(Stream requestStream)
    {
        try
        {
            var deserializerSettings = new JsonSerializerSettings
            {
                Converters =
                {
                    new ShortTimeOnlyJsonConverter(), new ShortDateOnlyJsonConverter(), new DayOfWeekJsonConverter(), new NullableShortDateOnlyJsonConverter()
                },

            };
            var body = await new StreamReader(requestStream).ReadToEndAsync();
            return JsonConvert.DeserializeObject<TRequest>(body, deserializerSettings);

        }
        catch (Exception ex)
        {
            throw new JsonRequestReadException("Problem reading request", ex);
        }
    }
}