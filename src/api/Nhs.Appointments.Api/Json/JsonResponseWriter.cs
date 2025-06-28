using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Serialization;

namespace Nhs.Appointments.Api.Json;

public static class JsonResponseWriter
{
    public static ContentResult WriteResult(object result, HttpStatusCode status = HttpStatusCode.OK)
    {
        var contentResult = new ContentResult();
        contentResult.StatusCode = (int)status;
        contentResult.ContentType = "application/json";
        contentResult.Content = Serialize(result);
        return contentResult;
    }

    public static string Serialize(object result)
    {
        var serializerSettings = new JsonSerializerSettings
        {
            Converters = { new ShortTimeOnlyJsonConverter(), new ShortDateOnlyJsonConverter(), new DayOfWeekJsonConverter(), new Newtonsoft.Json.Converters.StringEnumConverter { AllowIntegerValues = false } },
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false,
                    OverrideSpecifiedNames = true
                }
            }
        };
        return JsonConvert.SerializeObject(result, serializerSettings);
    }
}
