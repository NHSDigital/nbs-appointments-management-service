using System;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
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
    public static async Task<(IReadOnlyCollection<ErrorMessageResponseItem> errors, TRequest request)> ReadRequestAsync<TRequest>(Stream requestStream, bool checkSchemaFirst = false)
    {
        var deserializerSettings = new JsonSerializerSettings
        {
            Converters =
                {
                    new ShortTimeOnlyJsonConverter(), 
                    new ShortDateOnlyJsonConverter(), 
                    new DayOfWeekJsonConverter(), 
                    new NullableShortDateOnlyJsonConverter(),
                    new StrictBooleanJsonConverter(),
                    new Newtonsoft.Json.Converters.StringEnumConverter{AllowIntegerValues=false}
                },

        };
        var body = await new StreamReader(requestStream).ReadToEndAsync();

        if(checkSchemaFirst)
        {
            var schemaErrors = JsonToObjectSchemaValidation.ValidateConversion<TRequest>(body);
            if(schemaErrors.Any())
                return (schemaErrors, default(TRequest));
        }

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