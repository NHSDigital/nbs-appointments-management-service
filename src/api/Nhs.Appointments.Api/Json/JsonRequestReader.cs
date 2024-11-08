using System;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Nhs.Appointments.Api.Models;

namespace Nhs.Appointments.Api.Json;

public static class DateTimeFormats
{
    public static readonly String DateTime = "yyyy-MM-dd HH:mm";
    public static readonly String TimeOnly = "HH:mm";
    public static readonly String DateOnly = "yyyy-MM-dd";
}

public static class JsonRequestReader
{
    public static async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, TRequest request)> ReadRequestAsync<TRequest>(Stream requestStream)
    {
        var deserializerSettings = new JsonSerializerSettings
        {
            Converters =
                {
                    new ShortTimeOnlyJsonConverter(), new ShortDateOnlyJsonConverter(), new DayOfWeekJsonConverter(), new NullableShortDateOnlyJsonConverter()
                },

        };
        var body = await new StreamReader(requestStream).ReadToEndAsync();

        try
        {
            return (Enumerable.Empty<ErrorMessageResponseItem>().ToList().AsReadOnly(), JsonConvert.DeserializeObject<TRequest>(body, deserializerSettings));
        }
        catch (Exception)
        {
            return (JsonToObjectSchemaValidation.ValidateConversion<TRequest>(body), default(TRequest));            
        }
    }
}