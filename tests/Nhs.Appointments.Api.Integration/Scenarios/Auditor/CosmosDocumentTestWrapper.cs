using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Api.Integration.Scenarios.Auditor;
using System;
using System.Collections.Generic;

public class CosmosDocumentTestWrapper<T>
{
    [JsonExtensionData]
    private IDictionary<string, JToken> _additionalData;

    public T Document
    {
        get
        {
            var settings = new JsonSerializer();
            settings.Converters.Add(new TimeOnlyConverter());

            return JObject.FromObject(_additionalData).ToObject<T>(settings);
        }
    }

    [JsonProperty("_ts")]
    public long RawTimestamp { get; set; }

    public DateTimeOffset Timestamp => DateTimeOffset.FromUnixTimeSeconds(RawTimestamp);
}
